using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LuftHansa {

    public struct Csomag {
        public string azonosito;
        public double suly;
        public double terfogat;
    }

    public struct CsaladiCsomag {
        public string csaladAzonositot;
        public double osszsuly;
        public double osszterfogat;
        public List<Csomag> csomagok;
    }

    public struct kontener {
        public int azonosito;
        public int erokar;
        public double jelenlegiSuly;
        public double jelenlegiTerfogat;
        public List<CsaladiCsomag> betoltottCsaladiCsomagok;
    }



    internal class Program {



        static Dictionary<string,CsaladiCsomag> beolvasas(string path) {
            StreamReader sr = new StreamReader("../../" + path);
            Dictionary<string, CsaladiCsomag> eredmenyek = new Dictionary<string, CsaladiCsomag>();
            while (!sr.EndOfStream) {
                string[] darabok = sr.ReadLine().Split(';');
                Csomag cs = new Csomag();
                cs.azonosito = darabok[0];
                string csaladId = darabok[1];
                cs.suly = Convert.ToDouble(darabok[2].Replace('.', ','));
                cs.terfogat = Convert.ToDouble(darabok[3].Replace('.', ','));

                if (!eredmenyek.ContainsKey(csaladId)) {
                    CsaladiCsomag uj = new CsaladiCsomag();
                    uj.csaladAzonositot = csaladId;
                    uj.osszsuly = cs.suly;
                    uj.osszterfogat = cs.terfogat;
                    uj.csomagok = new List<Csomag>();
                    uj.csomagok.Add(cs);
                    eredmenyek[csaladId] = uj;
                    
                } else {
                    CsaladiCsomag modosit = eredmenyek[csaladId];
                    modosit.csomagok.Add(cs);
                    modosit.osszsuly += cs.suly;
                    modosit.osszterfogat += cs.terfogat;
                    eredmenyek[csaladId] = modosit;
                    
                }
            }
            sr.Close();

            return eredmenyek;
            
        }


        static kontener[] initKontener() {
            kontener[] kontenerek = new kontener[5];
            int[] erokarok = { -2, -1, 0, 1, 2 };
            for (int i = 0; i < 5; i++) {
                kontenerek[i].azonosito = i + 1;
                kontenerek[i].erokar = erokarok[i];
                kontenerek[i].jelenlegiSuly = 0;
                kontenerek[i].jelenlegiTerfogat = 0;
                kontenerek[i].betoltottCsaladiCsomagok = new List<CsaladiCsomag>();
            }
            return kontenerek;
        }

        static bool beleferE(kontener k,CsaladiCsomag cs) {
            return ((cs.osszsuly + k.jelenlegiSuly) <= 1500 && (cs.osszterfogat + k.jelenlegiTerfogat) <= 6.0);
        }

        static List<CsaladiCsomag> osszesitesListaba(Dictionary<string,CsaladiCsomag> csaladok) {
            List<CsaladiCsomag> csaladLista = new List<CsaladiCsomag>();
            foreach (KeyValuePair<string, CsaladiCsomag> adat in csaladok) {
                csaladLista.Add(adat.Value);
            }
            return csaladLista;
        }

        static void rendezes(List<CsaladiCsomag> csaladLista) {
            for (int i = 0; i < csaladLista.Count - 1; i++) {
                for (int j = i + 1; j < csaladLista.Count; j++) {
                    if (csaladLista[i].osszsuly < csaladLista[j].osszsuly) {
                        (csaladLista[i], csaladLista[j]) = (csaladLista[j], csaladLista[i]);
                    }
                }
            }
        }

        static double centerOfGravity(kontener[] k) {
            double suly = 0;
            double szorzat = 0;
            for (int i = 0; i < k.Length; i++) {
                szorzat += k[i].jelenlegiSuly * k[i].erokar;
                suly += k[i].jelenlegiSuly;

            }
            return szorzat / suly;
        }

        static void mohoAlgoritmus(Dictionary<string, CsaladiCsomag> csaladok, kontener[] k) {
            List<CsaladiCsomag> csaladLista = osszesitesListaba(csaladok);
            rendezes(csaladLista);
            foreach (CsaladiCsomag csalad in csaladLista) {
                int legjobbIndex = 0;
                double legjobbCG = double.MaxValue;
                for (int i = 0; i < k.Length; i++) {
                    if (beleferE(k[i], csalad)) {
                        double Szorzat = 0;
                        double suly = 0;
                        for (int j = 0; j < k.Length; j++) {
                            Szorzat += k[j].jelenlegiSuly * k[j].erokar;
                            suly += k[j].jelenlegiSuly;
                        }
                        Szorzat += csalad.osszsuly * k[i].erokar;
                        suly += csalad.osszsuly;
                        double ujCG = Szorzat / suly;
                        if (Math.Abs(ujCG) < Math.Abs(legjobbCG)) {
                            legjobbIndex = i;
                            legjobbCG = ujCG;
                        }
                    }
                }
                k[legjobbIndex].jelenlegiSuly += csalad.osszsuly;
                k[legjobbIndex].jelenlegiTerfogat += csalad.osszterfogat;
                k[legjobbIndex].betoltottCsaladiCsomagok.Add(csalad);
                
            }
        }

        static void Main(string[] args) {
            Dictionary<string, CsaladiCsomag> eredmenyek = beolvasas("adatok.txt");
            kontener[] kontenerek = initKontener();
            mohoAlgoritmus(eredmenyek, kontenerek);

            Console.WriteLine("LUFTHANSA JÁRAT RAKODÁSI TERV");
            Console.WriteLine();
            foreach (kontener k in kontenerek) {
                Console.WriteLine($"Konténer {k.azonosito} (Erőkar: {k.erokar}): {k.jelenlegiSuly} kg / {k.jelenlegiTerfogat} m^3 - {k.betoltottCsaladiCsomagok.Count} család");
            }

            double centerGravit = centerOfGravity(kontenerek);
            Console.WriteLine();
            Console.WriteLine($"A repülőgép VÉGSŐ súlypontja (CG): {centerGravit:F4}");
            if (Math.Abs(centerGravit) < 0.5) {
                Console.WriteLine("A gép tökéletes egyensúlyban van. Felszállás engedélyezve!");
            } else {
                Console.WriteLine("Nem fog felszállni");
            }
        }
    }
}
