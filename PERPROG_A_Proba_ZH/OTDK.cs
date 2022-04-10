using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PERPROG_A_Proba_ZH
{
    public class OTDK
    {
        static List<Ajtonallo> Ajtónállók = new List<Ajtonallo>();
        static object Ajtonallok_lock = new object();

        private List<Eloado> Előadók = new List<Eloado>();
        private List<Latogato> Látogatók = new List<Latogato>();

        public OTDK(int n, int k)
        { 
            for (int i = 0; i < n; i++)
            {
                Ajtónállók.Add(new Ajtonallo(i+1,new Terem(i+1)));
                Előadók.Add(new Eloado(i+1));
            }
            for (int i = 0; i < k; i++)
            {
                Látogatók.Add(new Latogato(i+1));
            }
            (new Task(()=> {
                while (true)
                {
                    System.Threading.Thread.Sleep(1000);
                    Console.Clear();
                    for (int i = 0; i < Ajtónállók.Count; i++)
                    {
                        Console.WriteLine(Ajtónállók[i].ToString());
                    }
                }
            },TaskCreationOptions.LongRunning)).Start();
        }

        public class Latogato
        {
            public int Érdeklődés;
            public Terem Terme;
            public int Id;
            public Latogato(int id)
            {
                Id = id;
                this.Terme = null;
                while (this.Terme == null)
                {
                    Ajtónállók[Program.rnd.Next(0, Ajtónállók.Count)].Beenged(this);
                    System.Threading.Thread.Sleep(10);
                }
                Érdeklődés = 100;

                (new Task(()=> {
                    while (true)
                    {
                        System.Threading.Thread.Sleep(1000);
                        if (Érdeklődés>0)
                        {
                            Érdeklődés -= Program.rnd.Next(1, 6);
                            if (Érdeklődés < 0)
                                Érdeklődés = 0;
                        }
                        else
                        {
                            while (this.Terme!=null)
                            {
                                lock (Ajtonallok_lock)
                                {
                                    Terme.Ajtónálló.Kienged(this);
                                }
                                System.Threading.Thread.Sleep(1000);
                            }
                            while (this.Terme==null)
                            {
                                lock (Ajtonallok_lock)
                                {
                                    Ajtónállók[Program.rnd.Next(0, Ajtónállók.Count)].Beenged(this);
                                }
                                System.Threading.Thread.Sleep(1000);
                            }
                            Érdeklődés = 100;
                        }
                    }
                },TaskCreationOptions.LongRunning)).Start();
            }
        }
        public class Eloado
        {
            public enum Allapotok
            {
                Felkészül, Előad, Diszkusszió, Tétlen
            }
            public Terem Terme;
            public int Id;
            public Allapotok Állapot { private set; get; }
            public Eloado(int id)
            {
                Állapot = Allapotok.Tétlen;
                Terme = null;
                Id = id;
                (new Task(()=> {
                    while (true)
                    {
                        lock (Ajtonallok_lock)
                        {
                            while (Terme==null)
                            {
                                for (int i = 0; i < Ajtónállók.Count; i++)
                                {
                                    Ajtónállók[i].Beenged(this);
                                    if (Terme!=null)
                                    {
                                        break;
                                    }
                                }
                                System.Threading.Thread.Sleep(50);
                            }
                        }
                        Állapot = Allapotok.Felkészül;
                        System.Threading.Thread.Sleep(Program.rnd.Next(750, 1251));
                        Állapot = Allapotok.Előad;
                        System.Threading.Thread.Sleep(Program.rnd.Next(14000, 16000));
                        Állapot = Allapotok.Diszkusszió;
                        System.Threading.Thread.Sleep(Program.rnd.Next(9000, 10000));
                        Terme.Előadó = null;
                        Terme = null;
                        Állapot = Allapotok.Tétlen;
                    }
                },TaskCreationOptions.LongRunning)).Start();
            }
        }
        public class Terem
        {
            public Eloado Előadó { get; set; }
            public List<Latogato> Látogatók;
            public Ajtonallo Ajtónálló;
            public int Id;
            public Terem(int id)
            {
                Előadó = null;
                Látogatók = new List<Latogato>();
                Ajtónálló = null;
                Id = id;
            }
        }
        public class Ajtonallo
        {
            public Terem Terme;
            public int Id;
            public Ajtonallo(int id,Terem terme)
            {
                Id = id;
                this.Terme = terme;
                terme.Ajtónálló = this;
            }
            public override string ToString()
            {
                string kimenet = "\nAjtónálló: " + Id;
                if (this.Terme!=null)
                {
                    kimenet += "\n Terme: " + Terme.Id;
                    if (this.Terme.Előadó != null)
                    {
                        kimenet += "\n Előadója: " + this.Terme.Előadó.Id+" Állapot: "+this.Terme.Előadó.Állapot.ToString()+" ";
                    }
                    kimenet+="\nLátogatók: ";
                    foreach (var item in this.Terme.Látogatók)
                    {
                        kimenet += item.Id +"("+item.Érdeklődés+"), ";
                    }
                }
                return kimenet;
            }
            public bool Beenged(Eloado e)
            {
                if (Terme.Előadó==null)
                {
                    Terme.Előadó = e;
                    e.Terme = this.Terme;
                    return true;
                }
                return false;
            }
            public void Beenged(Latogato l)
            {
                if (Terme.Előadó!=null)
                {
                    if (Terme.Előadó.Állapot!=Eloado.Allapotok.Előad)
                    {
                        Terme.Látogatók.Add(l);
                        l.Terme = this.Terme;
                    }
                }
            }
            public void Kienged(Latogato l)
            {
                if (Terme.Előadó != null)
                {
                    if (Terme.Előadó.Állapot != Eloado.Allapotok.Előad)
                    {
                        Terme.Látogatók.Remove(l);
                        l.Terme = null;
                    }
                }
                else
                {
                    Terme.Látogatók.Remove(l);
                    l.Terme = null;
                }
            }
        }
    }
}
