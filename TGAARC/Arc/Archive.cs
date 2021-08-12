using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ionic.Zlib;

namespace TGAARC.Arc
{
    public class Archive
    {
        private static string _diretorioDeArcsExtraidos = $"{AppContext.BaseDirectory}\\Extraidos\\";
        private static string _diretorioDeArcsNovos = $"{AppContext.BaseDirectory}\\Novos Arcs\\";
        public CabecalhoArc Cabecalho { get; set; }
        public List<PropriedadesArquivo> PropriedadesDeArquivos { get; set; }
        public int TamanhoDoPaddingPadding { get; set; }

        public Archive(string diretorio)
        {
            if (diretorio.Contains(".arc"))
                ObtenhaPropriedades(diretorio);
            else
                Recriar(diretorio);

        }

        public void ObtenhaPropriedades(string diretorio)
        {
            if (!Directory.Exists(_diretorioDeArcsExtraidos))
                Directory.CreateDirectory(_diretorioDeArcsExtraidos);
            
            BinaryReader br = new BinaryReader(File.OpenRead(diretorio));
            Cabecalho = new CabecalhoArc(br);
            
            PropriedadesDeArquivos = new List<PropriedadesArquivo>();
            for (int i = 0; i < Cabecalho.QuantidadeDeArquivos; i++)
                PropriedadesDeArquivos.Add(new PropriedadesArquivo(br));

            ExportarArquivos(br, Path.GetFileName(diretorio).Replace(".arc",""));
            br.Close();

        }

        public void ExportarArquivos(BinaryReader br, string diretorioArc)
        {
            int contador = 0;
            foreach (var propsDeArquivo in PropriedadesDeArquivos)
            {
                string nomeArquivo = contador.ToString().PadLeft(4,'0') + "_" + propsDeArquivo.Diretorio.Split('\\').Last();
                string caminhoExtracao = $"{_diretorioDeArcsExtraidos}{diretorioArc}\\{propsDeArquivo.Diretorio.Replace(propsDeArquivo.Diretorio.Split('\\').Last(), nomeArquivo)}{propsDeArquivo.Extensao}";
                CrieDiretorioParaExtracao(Path.GetDirectoryName(caminhoExtracao));                
                byte[] arquivo = LerArquivoEDescomprimir(br, propsDeArquivo.TamanhoComprimido, propsDeArquivo.Endereco);
                File.WriteAllBytes(caminhoExtracao, arquivo);
                contador++;
            }


        }

        private static void CrieDiretorioParaExtracao(string caminho)
        {
            if (!Directory.Exists(caminho))
                Directory.CreateDirectory(caminho);

        }

        private static byte[] LerArquivoEDescomprimir(BinaryReader br, int tamanho, int endereco)
        {
            br.BaseStream.Position = endereco;
            return ComprimirOuDescomprimir(br.ReadBytes(tamanho), CompressionMode.Decompress);

        }

        public void Recriar(string diretorio)
        {
         
            if (!Directory.Exists(_diretorioDeArcsNovos))
            {
                Directory.CreateDirectory(_diretorioDeArcsNovos);
            }


            RecriarArc(diretorio);


        }

        private void RecriarArc(string diretorio)
        {
            Directory.CreateDirectory($"{AppContext.BaseDirectory}\\temp");
            string nomeDoArc = new DirectoryInfo(diretorio).Name;
            string[] listaDeArquivos = Directory.GetFiles(diretorio, "*", SearchOption.AllDirectories).Where(d => ExtensoesArc.Extensoes.ContainsValue(Path.GetExtension(d))).OrderBy(d => Convert.ToUInt32(Path.GetFileName(d).Substring(0,4))).ToArray();
            int tamanhoDaTabela = listaDeArquivos.Length <= 227 ? 32760 : 65528;             
            string diretorioBase = diretorio.Replace(Path.GetFileName(diretorio), "");
            int enderecoBase = tamanhoDaTabela + 8;
            List<PropriedadesArquivo> propriedadesArquivos = GerarPropriedas(listaDeArquivos, diretorio, enderecoBase);
            CabecalhoArc cabecalhoArc = new() { Assinatura = "ARC\0", Versao = 7, QuantidadeDeArquivos = (ushort)listaDeArquivos.Length };
            MemoryStream ms = new MemoryStream();          

            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                cabecalhoArc.EscreverPropriedades(bw);
                int posicaoNaTabela = (int)bw.BaseStream.Position;
                foreach (var propriedades in propriedadesArquivos)
                {
                    bw.BaseStream.Position = posicaoNaTabela;
                    propriedades.EscreverPropriedades(bw);
                    posicaoNaTabela += 0x90;
                }
              
            }

            File.WriteAllBytes($"{_diretorioDeArcsNovos}{nomeDoArc}.arc", ms.ToArray());
            Directory.Delete($"{AppContext.BaseDirectory}\\temp", true);
        }

        private List<PropriedadesArquivo> GerarPropriedas(string[] listaDeCaminhosDeArquivos, string caminhoBase, int enderecoBase) 
        {
            List<PropriedadesArquivo> propriedades = new List<PropriedadesArquivo>();

            foreach (var caminho in listaDeCaminhosDeArquivos)
            {
                PropriedadesArquivo propriedadesArquivo = new PropriedadesArquivo(caminho, caminhoBase, enderecoBase);
                propriedades.Add(propriedadesArquivo);
                enderecoBase += propriedadesArquivo.TamanhoComprimido;
            }

            

            return propriedades;
        }



        public static byte[] ComprimirOuDescomprimir(byte[] bytes, CompressionMode mode)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (ZlibStream zip = new ZlibStream(ms, mode, true))
                {
                    zip.Write(bytes, 0, bytes.Length);
                }

                return ms.ToArray();
            }
        }

    }


}





