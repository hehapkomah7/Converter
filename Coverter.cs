using System;
using System.Xml.Linq;
using System.Text;
using System.IO;
using Microsoft.VisualBasic.FileIO;

namespace ConverterApp
{
    class Program
    {
        static void Main(string[] args)
        {
            //var FilePath = @"C:\Users\admin\Desktop\Converteds\New\Converted_Data_net.csv";

            //var FileOutputPath = @"C:\Users\admin\Desktop\new_doc.xml";


            var FilePath = args[0];

            var FileOutputPath = args[1];

            var Delimiter = args[2];

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var program = new Program();

            Console.WriteLine("var config = program.ReadConfig(FilePath);");

            var config = program.ReadConfig(FilePath, Delimiter);
            
            Encoding utf8WithoutBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

            //Console.WriteLine(config.ToString());

            Console.WriteLine("var sw = new StreamWriter(FileOutputPath, false, utf8WithoutBom)");

            using (
            var sw = new StreamWriter(FileOutputPath, false, utf8WithoutBom)
            )
            {
                sw.Write(config.ToString());
            }
        }

        XElement MakeConfig(TextFieldParser parser)
        {

            Console.WriteLine("XElement MakeConfig(TextFieldParser parser)");

            var converter = new converter();

            var config = new XElement("config");

            config.Add(converter.MakeJunctions(parser));

            config.Add(converter.MakeShoulders(parser));

            return config;

        }

        XElement ReadConfig(string FilePath, string Delimiter)
        {

            Console.WriteLine("XElement ReadConfig(string FilePath)_first_using");

            using (var sr = new StreamReader(FilePath, Encoding.GetEncoding("windows-1251")))
            {

                Console.WriteLine("XElement ReadConfig(string FilePath)_second_using");

                using (TextFieldParser readz = new TextFieldParser(sr))
                {
                    readz.TextFieldType = FieldType.Delimited;
                    readz.SetDelimiters(Delimiter);

                    var config = MakeConfig(readz);

                    return config;
                }
            }
        }
    }

    public class converter
    {
        XElement MakeCoeffitiens(string[] names, string[] values, int start, int end)
        {

            Console.WriteLine("MakeCoeff_Entry");

            var coefficients = new XElement("coefficients");

            for (int i = start; i < end; i++)
            {
                if (values[i] == "0") continue;

                var item = new XElement("item",
                    new XAttribute("vehicle", names[i]),
                    new XAttribute("value", "1")
                );
                coefficients.Add(item);
            }
            return coefficients;
        }

        XElement MakeCapacity(string[] names, string[] values, int start, int end)
        {

            var capacity = new XElement("capacity");

            for (int i = start; i < end; i++)
            {
                if (values[i] == "0") continue;

                var item = new XElement("item",
                    new XAttribute("vehicle", names[i]),
                    new XAttribute("value", values[i]));
                capacity.Add(item);
            }
            return capacity;
        }

            void AddProduct(XElement e, string product, string value)
        {
            e.Add(new XElement("item",
                new XAttribute("product", product),
                new XAttribute("value", value)));
        }
        XElement MakeLoading(string[] names, string[] values, int start, int end)
        {

            Console.WriteLine("XElement MakeLoading(string[] names, string[] values, int start, int end)");

            var loading = new XElement("loading");

            for (int i = start; i < end; i++)
            {
                if (values[i] == "0") continue;

                AddProduct(loading, names[i], values[i]);
            }

            return loading;
        }

        XElement MakeJunction(string[] names, string[] values)
        {
            var coefficients = MakeCoeffitiens(names, values, 5, 10);

            var capacity = MakeCapacity(names, values, 5, 10);

            var loading = MakeLoading(names, values, 10, names.Length);

            var junction = new XElement("junction",
                new XAttribute("code", values[0]),
                new XAttribute("name", values[1]),
                new XAttribute("latitude", values[2]),
                new XAttribute("longitude", values[3]),
                new XAttribute("grid", values[4]),
                    coefficients,
                    capacity,
                    loading
                );

            return junction;
        }

        public XElement MakeJunctions(TextFieldParser parser)
        {

            Console.WriteLine("public XElement MakeJunctions(TextFieldParser parser)");

            var names = parser.ReadFields();

            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] == "")
                {
                    Array.Resize(ref names, i);
                    break;
                }
            }

            var junctions = new XElement("junctions");

            while (!parser.EndOfData)
            {
                string[] values = parser.ReadFields();

                if (values[0] == "")
                {
                    break;
                }

                junctions.Add(MakeJunction(names, values));
            }

            return junctions;
        }

        XElement MakeShoulder(string[] values)
        {
            var shoulder = new XElement("shoulder",
                new XAttribute("junction1", values[0]),
                new XAttribute("junction2", values[1]),
                new XAttribute("vehicle", values[2]),
                new XAttribute("capacity", values[3]),
                new XAttribute("distance", values[4]),
                new XAttribute("unit", values[5]),
                new XAttribute("tariff", values[6])
                );

            return shoulder;
        }

        public XElement MakeShoulders(TextFieldParser parser)
        {

            Console.WriteLine("public XElement MakeShoulders(TextFieldParser parser)");

            var names = parser.ReadFields();

            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] == "")
                {
                    Array.Resize(ref names, i);
                    break;
                }
            }

            var shoulders = new XElement("shoulders");

            while (!parser.EndOfData)
            {
                string[] values = parser.ReadFields();

                if (values[0] == "")
                {
                    break;
                }

                shoulders.Add(MakeShoulder(values));
            }

            return shoulders;
        }

    }

}
