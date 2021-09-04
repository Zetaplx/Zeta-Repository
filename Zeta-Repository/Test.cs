using System;
using System.Collections.Generic;
using System.Text;
using Zeta.Generics;
using System.Dynamic;

namespace Zeta_Repository
{
    class Test
    {

        void Testy()
        {
            var rolo = new Rolodex().AsDynamic();

            var v = rolo.test;
        }
    }
}
