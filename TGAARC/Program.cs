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
