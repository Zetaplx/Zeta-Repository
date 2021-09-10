using System;
using System.Collections.Generic;
using System.Text;
using Zeta.Generics;

namespace Zeta.CSM
{
    public class StateMachine
    {
        public const string Splitter = "_";
        public const string Default = "Default";
        public const string Start = "Start";


        Rolodex State;
        Dictionary<string, Node> AllNodes;
        Dictionary<string, List<(string node, string gate)>> Connections;

        List<string> QuedMessages;

        public StateMachine()
        {
            State = new Rolodex();
            AllNodes = new Dictionary<string, Node>();
            Connections = new Dictionary<string, List<(string node, string gate)>>();

            QuedMessages = new List<string>();
        }

        public void Initialize()
        {
            foreach (var node in AllNodes) node.Value.Initialize(this);
            SendMessage(Start);
        }

        public bool Push<T>(string name, Func<T> get, Action<T> set) => State.Register(name, get, set);
        public bool Push<T>(string name, T t) => State.Push(name, t);

        public void Update()
        {
            foreach (var active in AllNodes) if (active.Value.Active) active.Value.DoUpdate(State);
            ProcessMessages();
        }
        public void FixedUpdate()
        {
            foreach (var active in AllNodes) if (active.Value.Active) active.Value.DoFixedUpdate(State);
            ProcessMessages();
        }

        public void ProcessMessages()
        {
            foreach (var message in QuedMessages)
            {
                if (Connections.TryGetValue(message, out var v))
                {
                    foreach (var target in v)
                    {
                        if (AllNodes.TryGetValue(target.node, out var tNode))
                        {
                            tNode.DoEnter(State, target.gate);
                        }
                    }
                }
            }
            QuedMessages.Clear();
        }

        public void SendMessage(string message)
        {
            QuedMessages.Add(message);
        }
        public void SendFromGate(string node, string gate = Default)
        {
            SendMessage(node + Splitter + gate);
        }
        public void SendToState(string oNode, string oGate, string iNode, string iGate)
        {
            var key = oNode + Splitter + oGate;
            if(!Connections.TryGetValue(key, out var con))
            {
                List<(string node, string gate)> list = new List<(string node, string gate)>();
                list.Add((iNode, iGate));
                Connections.Add(key, list);
            } else if(!con.Contains((iNode, iGate)))
            {
                con.Add((iNode, iGate));
            }

            SendFromGate(oNode, oGate);
        }

        public StateMachine AddNode(Node node)
        {
            if (!AllNodes.TryGetValue(node.Name, out var v)) { AllNodes.Add(node.Name, node); node.Initialize(this); return this; }
            throw new System.InvalidOperationException($"A node with name {node.Name} already present in State Machine.");
        }
        public bool RemoveNode(string name)
        {
            return AllNodes.Remove(name);
        }

        public StateMachine AddConnection(string outNode, string outGate, string inNode, string inGate = Default)
        {
            var key = outNode + Splitter + outGate;
            if (Connections.TryGetValue(key, out var l)) { l.Add((inNode, inGate)); return this; }
            var newL = new List<(string, string)>();
            newL.Add((inNode, inGate));
            Connections.Add(key, newL);
            return this;
        }
        public StateMachine AddConnection(string outNode, string inNode)
        {
            var outGate = Default;
            var inGate = Default;
            var key = outNode + Splitter + outGate;
            if (Connections.TryGetValue(key, out var l)) { l.Add((inNode, inGate)); return this; }
            var newL = new List<(string, string)>();
            newL.Add((inNode, inGate));
            Connections.Add(key, newL);
            return this;
        }

        public bool RemoveConnection(string outNode, string outGate, string inNode, string inGate = Default)
        {
            var key = outNode + Splitter + outGate;
            if (Connections.TryGetValue(key, out var l))
            {
                var b = l.Remove((inNode, inGate));
                if (l.Count == 0)
                {
                    return Connections.Remove(key);
                }
                return b;
            }
            return false;
        }
        public StateMachine AddMessageListener(string message, string inNode, string inGate = Default)
        {
            if (Connections.TryGetValue(message, out var l)) { l.Add((inNode, inGate)); return this; }
            var newL = new List<(string, string)>();
            newL.Add((inNode, inGate));
            Connections.Add(message, newL);
            return this;
        }
        public bool RemoveMessageListener(string message, string inNode, string inGate)
        {
            if (Connections.TryGetValue(message, out var l))
            {
                var b = l.Remove((inNode, inGate));
                if (l.Count == 0)
                {
                    return Connections.Remove(message);
                }
                return b;
            }

            return false;
        }

    }
    public class Node
    {
        public string Name;
        protected Rolodex Stack;
        StateMachine Machine;

        public delegate void NodeAction(Rolodex state, Rolodex stack);
        public event NodeAction OnEnter;
        public event NodeAction OnExit;
        public event NodeAction OnUpdate;
        public event NodeAction OnFixedUpdate;

        public Dictionary<string, NodeAction> IGateActions = new Dictionary<string, NodeAction>();
        public Dictionary<string, NodeAction> OGateActions = new Dictionary<string, NodeAction>();

        public bool Active { get; private set; }

        public Node(string name, StateMachine machine)
        {
            Stack = new Rolodex();

            Name = name;

            Stack.Register("Name", () => Name, (d) => Name = d);
            Stack.Push("Node", this);

            Initialize(machine);
        }

        public Node(string name)
        {
            Stack = new Rolodex();

            Name = name;

            Stack.Register("Name", () => Name, (d) => Name = d);
            Stack.Push("Node", this);
        }

        public void Initialize(StateMachine machine) => Machine = machine;

        public virtual void Enter(Rolodex state) { }
        public virtual void Update(Rolodex state) { }
        public virtual void FixedUpdate(Rolodex state) { }
        public virtual void Exit(Rolodex state) { }

        public void DoEnter(Rolodex state, string iGate = StateMachine.Default)
        {
            Active = true;

            if (IGateActions.TryGetValue(iGate, out var v)) v.Invoke(state, Stack);

            OnEnter?.Invoke(state, Stack);
            Enter(state);
        }
        public void DoUpdate(Rolodex state)
        {
            OnUpdate?.Invoke(state, Stack);

            Update(state);
        }
        public void DoFixedUpdate(Rolodex state)
        {
            OnFixedUpdate?.Invoke(state, Stack);

            FixedUpdate(state);
        }
        public void DoExit(Rolodex state, string oGate = StateMachine.Default, bool exit = true)
        {
            Active = !exit;



            OnExit?.Invoke(state, Stack);
            Exit(state);

            if (OGateActions.TryGetValue(oGate, out var v)) v.Invoke(state, Stack);
        }

        public void SendFromGate(Rolodex state, string gate = StateMachine.Default, bool exit = true) {
            DoExit(state, gate, exit);
            Machine.SendFromGate(Name, gate); 
        }
        public void SendMesssage(string message) => Machine.SendMessage(message);
        public void SendToState(Rolodex state, string targetName, string oGate = StateMachine.Default, string iGate = StateMachine.Default, bool exit = true)
        {
            DoExit(state, oGate, exit);
            Machine.SendToState(Name, oGate, targetName, iGate);
        }


        public enum ActionType { ENTER, EXIT, UPDATE, FIXEDUPDATE, IGATE, OGATE }
        public const ActionType EnterType = ActionType.ENTER;
        public const ActionType ExitType = ActionType.EXIT;
        public const ActionType UpdateType = ActionType.UPDATE;
        public const ActionType FixedUpdateType = ActionType.FIXEDUPDATE;

        public Node AddBaseAction(ActionType type, NodeAction action)
        {
            Action<NodeAction> addMethod = null;
            switch (type)
            {
                case EnterType:
                    addMethod = AddEnterAction;
                    break;
                case ExitType:
                    addMethod = AddExitAction;
                    break;
                case UpdateType:
                    addMethod = AddUpdateAction;
                    break;
                case FixedUpdateType:
                    addMethod = AddFixedUpdateAction;
                    break;
            }
            addMethod?.Invoke(action);
            return this;
        }
        public Node AddBaseActions(ActionType type, params NodeAction[] actions)
        {
            foreach (var action in actions) AddBaseAction(type, action);
            return this;
        }
        public Node SetIGateAction(string iGate, NodeAction action)
        {
            SetIGate(iGate, action);
            return this;
        }
        public Node SetOGateAction(string oGate, NodeAction action)
        {
            SetOGate(oGate, action);
            return this;
        }

        void AddEnterAction(NodeAction action) { OnEnter += action; }
        void AddExitAction(NodeAction action) { OnExit += action; }
        void AddUpdateAction(NodeAction action) { OnUpdate += action; }
        void AddFixedUpdateAction(NodeAction action) { OnFixedUpdate += action; }
        void SetIGate(string iGate, NodeAction action) { if (!IGateActions.TryGetValue(iGate, out var a)) IGateActions.Add(iGate, action); }
        void SetOGate(string oGate, NodeAction action) { if (!OGateActions.TryGetValue(oGate, out var a)) OGateActions.Add(oGate, action); }

    }
}
