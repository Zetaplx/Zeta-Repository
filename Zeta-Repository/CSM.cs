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
        Dictionary<string, Node> TotalNodes;
        Dictionary<string, List<(string node, string gate)>> Connections;
        public HashSet<string> ActiveNodes;

        List<string> QuedMessages;

        public StateMachine()
        {
            State = new Rolodex();
            TotalNodes = new Dictionary<string, Node>();
            Connections = new Dictionary<string, List<(string node, string gate)>>();
            ActiveNodes = new HashSet<string>();

            QuedMessages = new List<string>();
        }

        public void Initialize()
        {
            foreach (var node in TotalNodes) node.Value.Initialize(this);
            SendMessage(Start);
        }

        public bool Push<T>(string name, Func<T> get, Action<T> set) => State.Register(name, get, set);
        public bool Push<T>(string name, T t) => State.Push(name, t);

        public void Update()
        {
            foreach (var active in ActiveNodes) if (TotalNodes.TryGetValue(active, out var node)) node.Update(State);
            ProcessMessages();
        }

        public void FixedUpdate()
        {
            foreach (var active in ActiveNodes) if (TotalNodes.TryGetValue(active, out var node)) node.FixedUpdate(State);
            ProcessMessages();
        }


        public void ProcessMessages()
        {
            foreach (var message in QuedMessages)
            {
                var splitMessage = message.Split(new string[] { Splitter }, StringSplitOptions.None);
                if (splitMessage.Length >= 2)
                    if (TotalNodes.TryGetValue(splitMessage[0], out var n)) { n.Exit(State, splitMessage[1]); ActiveNodes.Remove(n.Name); }
                if (Connections.TryGetValue(message, out var v))
                {
                    foreach (var target in v)
                    {
                        if (TotalNodes.TryGetValue(target.node, out var tNode))
                        {
                            tNode.Enter(State, target.gate);

                            ActiveNodes.Add(target.node);
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

        public void AddNode(Node node)
        {
            if (!TotalNodes.TryGetValue(node.Name, out var v)) { TotalNodes.Add(node.Name, node); node.Initialize(this); return; }
            throw new System.InvalidOperationException($"A node with name {node.Name} already present in State Machine.");
        }
        public bool RemoveNode(string name)
        {
            return TotalNodes.Remove(name);
        }

        public void AddConnection(string outNode, string outGate, string inNode, string inGate = Default)
        {
            var key = outNode + Splitter + outGate;
            if (Connections.TryGetValue(key, out var l)) { l.Add((inNode, inGate)); return; }
            var newL = new List<(string, string)>();
            newL.Add((inNode, inGate));
            Connections.Add(key, newL);
        }

        public void AddConnection(string outNode, string inNode)
        {
            var outGate = Default;
            var inGate = Default;
            var key = outNode + Splitter + outGate;
            if (Connections.TryGetValue(key, out var l)) { l.Add((inNode, inGate)); return; }
            var newL = new List<(string, string)>();
            newL.Add((inNode, inGate));
            Connections.Add(key, newL);
        }

        public void RemoveConnection(string outNode, string outGate, string inNode, string inGate = Default)
        {
            var key = outNode + Splitter + outGate;
            if (Connections.TryGetValue(key, out var l))
            {
                l.Remove((inNode, inGate));
                if (l.Count == 0)
                {
                    Connections.Remove(key);
                }
            }
        }
        public void AddMessageListener(string message, string inNode, string inGate = Default)
        {
            if (Connections.TryGetValue(message, out var l)) { l.Add((inNode, inGate)); return; }
            var newL = new List<(string, string)>();
            newL.Add((inNode, inGate));
            Connections.Add(message, newL);
        }
        public void RemoveMessageListener(string message, string inNode, string inGate)
        {
            if (Connections.TryGetValue(message, out var l))
            {
                l.Remove((inNode, inGate));
                if (l.Count == 0)
                {
                    Connections.Remove(message);
                }
            }
        }

    }
    public class Node
    {
        public string Name;
        protected Rolodex Stack;
        StateMachine Machine;

        public delegate void NodeAction(Rolodex state, Rolodex stack);
        public List<NodeAction> EnterActions;
        public List<NodeAction> ExitActions;
        public List<NodeAction> UpdateActions;
        public List<NodeAction> FixedUpdateActions;

        public Dictionary<string, NodeAction> IGateActions = new Dictionary<string, NodeAction>();
        public Dictionary<string, NodeAction> OGateActions = new Dictionary<string, NodeAction>();

        public Node(string name, StateMachine machine)
        {
            Stack = new Rolodex();
            EnterActions = new List<NodeAction>();
            ExitActions = new List<NodeAction>();
            UpdateActions = new List<NodeAction>();
            FixedUpdateActions = new List<NodeAction>();

            Name = name;

            Stack.Register("Name", () => Name, (d) => Name = d);
            Stack.Push("Node", this);

            Initialize(machine);
        }

        public Node(string name)
        {
            Stack = new Rolodex();
            EnterActions = new List<NodeAction>();
            ExitActions = new List<NodeAction>();
            UpdateActions = new List<NodeAction>();
            FixedUpdateActions = new List<NodeAction>();

            Name = name;

            Stack.Register("Name", () => Name, (d) => Name = d);
            Stack.Push("Node", this);
        }

        public void Initialize(StateMachine machine) => Machine = machine;

        public virtual void OnEnter(Rolodex state)       { }
        public virtual void OnUpdate(Rolodex state)      { }
        public virtual void OnFixedUpdate(Rolodex state) { }
        public virtual void OnExit(Rolodex state)        { }

        public void Enter(Rolodex state, string iGate = StateMachine.Default)
        {
            if (IGateActions.TryGetValue(iGate, out var v)) v.Invoke(state, Stack);

            foreach (var action in EnterActions) { action.Invoke(state, Stack); }

            OnEnter(state);
        }
        public void Update(Rolodex state)
        {
            foreach(var action in UpdateActions) { action.Invoke(state, Stack); }

            OnUpdate(state);
        }
        public void FixedUpdate(Rolodex state)
        {
            foreach (var action in FixedUpdateActions) { action.Invoke(state, Stack); }

            OnFixedUpdate(state);
        }
        public void Exit(Rolodex state, string oGate = StateMachine.Default)
        {
            if (OGateActions.TryGetValue(oGate, out var v)) v.Invoke(state, Stack);

            foreach (var action in ExitActions) { action.Invoke(state, Stack); }

            OnExit(state);
        }

        public void SendFromGate(string gate = StateMachine.Default) => Machine.SendFromGate(Name, gate);
        public void SendMesssage(string message) => Machine.SendMessage(message);
        public void SendToState(string stateName, string oGate = StateMachine.Default, string iGate = StateMachine.Default) => Machine.SendToState(Name, oGate, stateName, iGate);


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

        void AddEnterAction(NodeAction action) { EnterActions.Add(action); }
        void AddExitAction(NodeAction action) { ExitActions.Add(action); }
        void AddUpdateAction(NodeAction action) { UpdateActions.Add(action); }
        void AddFixedUpdateAction(NodeAction action) { FixedUpdateActions.Add(action); }
        void SetIGate(string iGate, NodeAction action) { if (!IGateActions.TryGetValue(iGate, out var a)) IGateActions.Add(iGate, action); }
        void SetOGate(string oGate, NodeAction action) { if (!OGateActions.TryGetValue(oGate, out var a)) OGateActions.Add(oGate, action); }

    }
}
