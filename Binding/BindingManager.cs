using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using kOS.Context;

namespace kOS.Binding
{
    public class BindingManager
    {
        private readonly List<Binding> bindings = new List<Binding>();
        
        public delegate void BindingSetDlg      (ICPU cpu, object val);
        public delegate object BindingGetDlg    (ICPU cpu);

        public BindingManager(ICPU cpu, String context)
        {
            Cpu = cpu;

            var contexts = new string[1];
            contexts[0] = context;

            foreach (var t in Assembly.GetExecutingAssembly().GetTypes())
            {
                var attr = (KOSBinding)t.GetCustomAttributes(typeof(KOSBinding), true).FirstOrDefault();

                if (attr == null || attr.Contexts.Any() && !attr.Contexts.Intersect(contexts).Any()) continue;
                var b = (Binding)Activator.CreateInstance(t);
                b.AddTo(this);
                bindings.Add(b);
            }
        }

        public ICPU Cpu { get; set; }

        public void AddGetter(String name, BindingGetDlg dlg)
        {
            var v = Cpu.FindVariable(name) ?? Cpu.FindVariable(name.Split(":".ToCharArray())[0]);

            if (v != null)
            {
                var variable = v as BoundVariable;
                if (variable != null)
                {
                    variable.Get = dlg;
                }
            }
            else
            {
                var bv = Cpu.CreateBoundVariable(name);
                bv.Get = dlg;
                bv.Cpu = Cpu;
            }
        }

        public void AddSetter(String name, BindingSetDlg dlg)
        {
            var v = Cpu.FindVariable(name.ToLower());
            if (v != null)
            {
                var variable = v as BoundVariable;
                if (variable != null)
                {
                    variable.Set = dlg;
                }
            }
            else
            {
                var bv = Cpu.CreateBoundVariable(name.ToLower());
                bv.Set = dlg;
                bv.Cpu = Cpu;
            }
        }

        public void Update(float time)
        {
            foreach (var b in bindings)
            {
                b.Update(time);
            }
        }
    }
}