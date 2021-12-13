using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Picae
{
    class Program
    {
        const int N = 500;
        const int START = 150;
        const int MINLENGTH = 5;

        static string[] Tokenize(string txt)
        {
            return txt.Split(' ');
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Què vols fer?");
            Console.WriteLine("1) Generar fitxer tsv (a partir de fitxer picae.json)");
            Console.WriteLine("2) Analitzar fitxer tsv (a partir del fitxer results.tsv)");
            Console.WriteLine("3) Generar fitxers tensor flow (a partir del fitxer results.tsv)");
            var a = Console.Read();

            if (a == 49)
            {
                RunTxt();
                //RunMongo();
            }
            else if (a == 50)
            {
                var l = new List<Tuple<string, string>>();
                l.Add(Tuple.Create("Un any de COVID-19", "Medicina 2014"));
                l.Add(Tuple.Create("La fàbrica gòtica", "Catedrals gòtiques"));
                l.Add(Tuple.Create("Arquitectura romànica tardana: fi del segle XII i segle XIII", "L’arquitectura religiosa d’època preromànica i romànica"));
                l.Add(Tuple.Create("Les llengües del món", "Les llengües amenaçades"));
                l.Add(Tuple.Create("El planeta blau", "La vida, patrimoni de la Terra?"));
                l.Add(Tuple.Create("Gerard Piqué i Bernabeu", "Lionel Andrés Messi"));

                l.Add(Tuple.Create("Club de Futbol Lleida", "Girona Club de Futbol"));
                l.Add(Tuple.Create("Copa del Món de futbol", "Copa Amèrica"));
                //l.Add(Tuple.Create("mastologia", "carcinologia"));
                //l.Add(Tuple.Create("Bernat de Granollacs", "Pere Pintor"));
                l.Add(Tuple.Create("microbiologia", "embriologia"));
                l.Add(Tuple.Create("Apple Inc.", "Microsoft"));
                l.Add(Tuple.Create("Jordi Bonareu i Bussot", "Enric Margall i Tauler"));
                l.Add(Tuple.Create("Pau Gasol i Sáez", "Marc Gasol i Sáez"));
                l.Add(Tuple.Create("General Motors Company", "Toyota Motor Company Limited"));
                //l.Add(Tuple.Create("Manfredo Tafuri", "Adolf Loos"));

                l.Add(Tuple.Create("Johann Wolfgang Amadeus Mozart", "Ludwig van Beethoven"));
                l.Add(Tuple.Create("Barack Hussein Obama", "Joe Biden"));
                l.Add(Tuple.Create("Tom Hanks", "Robin Williams"));
                l.Add(Tuple.Create("Nike", "Adidas"));
                l.Add(Tuple.Create("Bayerische Motoren Werke", "Porsche"));
                l.Add(Tuple.Create("National Aeronautics and Space Administration", "Estació Espacial Internacional"));
                l.Add(Tuple.Create("Banc Sabadell", "Banco Santander"));
                l.Add(Tuple.Create("Adolf Hitler", "Benito Mussolini"));
                l.Add(Tuple.Create("Pablo Ruiz Picasso", "Juan Gris"));
                l.Add(Tuple.Create("Antonio Machado Ruiz", "Miguel de Unamuno y Jugo"));
                Check(l);
            }
            else if (a == 51)
            {
                TF();
            }
            else
                Console.WriteLine("Què??");
        }

        static void TF()
        {
            StreamReader f = new StreamReader("results.tsv");
            StreamWriter f1 = new StreamWriter("vectors.tsv", false);
            StreamWriter f2 = new StreamWriter("metadata.tsv", false);
            int n = 0;
            while (!f.EndOfStream)
            {
                var linia = f.ReadLine();
                f1.WriteLine(linia.Substring(linia.IndexOf("\t")+1));
                f2.WriteLine(linia.Substring(0, linia.IndexOf("\t")));
                if (n++ > 1000000)
                    break;
            }
            f.Close();
            f1.Close();
            f2.Close();
        }

        static double Similitud(List<double> item1, List<double> item2)
        {
            double similitud = 0;
            for (var i = 0; i< item1.Count; i++)
            {
                if (item1[i] > 0 && item2[i] > 0)
                {
                    similitud += 0.0042; // magic number
                    similitud += Math.Min(item1[i], item2[i]);
                }
            }
            return similitud;
        }

        static void Check(List<Tuple<string, string>> list)
        {
            Console.Write("Loading tsv..." + Environment.NewLine);

            var f = new StreamReader("F:\\Escritorio\\Picae\\Picae\\bin\\Debug\\results.tsv");
            var dic = new Dictionary<string, List<double>>();
            while (!f.EndOfStream)
            {
                var linia = f.ReadLine();
                var camps = linia.Split('\t');
                if (!dic.ContainsKey(camps[0]))
                {
                    var valors = new List<double>();
                    for (int i = 1; i < camps.Length; i++) valors.Add(double.Parse(camps[i].Replace(".",",")));
                    dic.Add(camps[0], valors);
                }
            }
            f.Close();

            Console.Write("------------------------------" + Environment.NewLine);
            foreach (var pair in list)
            {
                Console.Write("Analyzing pair " + pair.Item1 + " <-> " + pair.Item2 + Environment.NewLine);
                var similituds = new List<double>();
                foreach (var item in dic)
                {
                    if (pair.Item1 != item.Key)
                    {
                        var similitud = Similitud(dic[pair.Item1], item.Value);
                        similituds.Add(similitud);
                    }
                }

                var similitud2 = Similitud(dic[pair.Item1], dic[pair.Item2]);
                similituds.Sort();

                var pos = 0;
                while (similitud2 > similituds[pos] && pos+1<similituds.Count) pos++;
                Console.Write("Similarity = " + Math.Round(similitud2, 2) + Environment.NewLine);
                Console.Write("Position = " + (similituds.Count - pos) + "/" + similituds.Count + " (" + Math.Round((double)pos * 100/similituds.Count, 2) + "%)" + Environment.NewLine);
                Console.Write("Most similar = " + Math.Round(similituds.Last(), 2) + Environment.NewLine);
                var sim = similituds[similituds.Count - 6];
                var more = new List<Tuple<string, double>>();
                foreach (var item in dic)
                {
                    if (pair.Item1 != item.Key)
                    {
                        var similitud = Similitud(dic[pair.Item1], item.Value);
                        if (similitud >= sim && similitud > 0)
                            more.Add(Tuple.Create(item.Key, similitud));
                    }
                }
                more.Sort(new Comparison<Tuple<string, double>>((b1, a1) => Math.Sign(a1.Item2 - b1.Item2)));
                var max = 10;
                foreach (var tup in more)
                {
                    Console.Write("More similar = " + tup.Item1 + " (" + Math.Round(tup.Item2, 2) + ")" + Environment.NewLine);
                    if (max-- < 0) break;
                }
                Console.Write("------------------------------" + Environment.NewLine);
            }
        }

        static void RunTxt()
        {
            var i = 0;
            var words = new Dictionary<string, int>();
            var f = new StreamReader("F:\\Escritorio\\picae.json");
            var documents = new List<Tuple<string, string, Dictionary<string, int>, List<double>>>();
            while (!f.EndOfStream)
            {
                i++;
                var linia = f.ReadLine();
                var index = linia.IndexOf("\"title\"");
                index = linia.IndexOf(":", index) + 2;
                var title = linia.Substring(index, linia.IndexOf("\"", index) - index);

                index = linia.IndexOf("\"normalized\"");
                index = linia.IndexOf(":", index) + 2;
                var normalized = linia.Substring(index, linia.IndexOf("\"", index) - index);

                var doc = new Tuple<string, string, Dictionary<string, int>, List<double>>(title, normalized, new Dictionary<string, int>(), new List<double>());
                documents.Add(doc);

                foreach (var word in Tokenize(normalized))
                {
                    if (!words.ContainsKey(word)) words.Add(word, 0);
                    words[word]++;
                    if (!doc.Item3.ContainsKey(word)) doc.Item3.Add(word, 0);
                    doc.Item3[word]++;
                }

                if (i % 2000 == 0)
                {
                    Console.Write("Processed " + i + Environment.NewLine);
                }
            }
            f.Close();

            var ordered_words = words.ToList();
            ordered_words.Sort(new Comparison<KeyValuePair<string, int>>((b1, a1) => Math.Sign(a1.Value - b1.Value)));

            var filtered_words = new List<string>();
            i = START;
            while (filtered_words.Count < N)
            {
                var word = ordered_words[i];
                if (word.Key.Length > MINLENGTH && !word.Key.Contains("’") && !word.Key.Contains("\\"))
                {
                    filtered_words.Add(word.Key);
                }
                i++;
            }
            var fw = new StreamWriter("results-columns.tsv");
            foreach (var w in filtered_words)
            {
                fw.WriteLine(w);
            }
            fw.Close();

            var max = -1;
            Console.Write("Generating tsv " + Environment.NewLine);
            fw = new StreamWriter("results.tsv");
            i = 0;
            foreach (var doc in documents)
            {
                i++;
                fw.Write(doc.Item1.Replace("\t", " "));
                foreach (var w in filtered_words)
                {
                    var pes = 0;
                    if (doc.Item3.ContainsKey(w))
                    {
                        pes = doc.Item3[w];
                    }
                    fw.Write("\t" + Math.Round((double)pes / 700, 6).ToString().Replace(",", "."));
                    if (pes > max) max = pes;
                }
                fw.WriteLine();
                if (i % 2000 == 0)
                {
                    Console.Write("Processed " + i + Environment.NewLine);
                }
            }
            fw.Close();
            Console.Write("Max: " + max + Environment.NewLine);
            Console.Write("Finished" + Environment.NewLine);
        }

        static void RunMongo()
        {
            var conString = "mongodb://localhost:27017?socketTimeoutMS=200000&connectTimeoutMS=200000&waitqueuetimeoutms=200000";
            var client = new MongoClient(conString);
            var db = client.GetDatabase("picae");
            var collection = db.GetCollection<BsonDocument>("products");

            var words = new Dictionary<string, int>();

            Console.Write("Loading documents..." + Environment.NewLine);
            var results = collection.Find(x => true).Skip(0).Limit(1000).ToList();
            var i = 0;
            var n = results.Count;
            var prev = -1;
            foreach (var result in results)
            {
                var perc = (double)i / n;
                if ((int)(perc * 10000) != prev)
                {
                    Console.Write("Reading documents... (" + perc + "%)" + Environment.NewLine);
                }
                var title = result.GetValue("title").ToString();
                var normalized = result.GetValue("normalized").ToString();
                foreach (var word in normalized.Split(' '))
                {
                    if (!words.ContainsKey(word)) words.Add(word, 0);
                    words[word]++;
                }
                i++;
            }

            var ordered_words = words.ToList();
            ordered_words.Sort(new Comparison<KeyValuePair<string, int>>((b1, a1) => Math.Sign(a1.Value - b1.Value)));

        }
    }
}
