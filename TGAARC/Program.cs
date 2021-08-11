using System;
using System.IO;
using System.Linq;
using TGAARC.Arc;

namespace TGAARC
{
    class Program
    {
        static void Main(string[] args)
        {
           // args = new string[1];
            //args[0] = "table";
            //args[0] = @"C:\Users\djmat\Desktop\font_eng";
            foreach (var caminho in args)
            {
                if (caminho.Contains(".arc"))
                    Console.WriteLine($"Exportando {Path.GetFileName(caminho)}");               
                else
                    Console.WriteLine($"Recriando {caminho.Split("\\").Last()}");
               
                Archive arc = new Archive(caminho);

            }
        }


    }
}
