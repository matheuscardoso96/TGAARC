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
        public CabecalhoArc Cabecalho { get; set; }
        public List<PropriedadesArquivo> PropriedadesDeArquivos { get; set; }
        public List<string> ListaDeCArquivos { get; set; }
        public int TamanhoDoPaddingPadding { get; set; }

        public Archive(string diretorio)
        {
            if (diretorio.Contains(".arc"))
                Exportar(diretorio);
            else
                Recriar(diretorio);

        }

        public void Exportar(string diretorio)
        {
            BinaryReader br = new BinaryReader(File.OpenRead(diretorio));
            Cabecalho = new CabecalhoArc(br);

            PropriedadesDeArquivos = new List<PropriedadesArquivo>();

            for (int i = 0; i < Cabecalho.QuantidadeDeArquivos; i++)
            {
                PropriedadesDeArquivos.Add(new PropriedadesArquivo(br));
            }

            TamanhoDoPaddingPadding = PropriedadesDeArquivos[0].Endereco - (int)(br.BaseStream.Position);

            ListaDeCArquivos = new List<string>();
            ExportarArquivos(br, Path.GetFileName(diretorio.Replace(".arc", "")));
            br.Close();

        }

        public void Recriar(string diretorio)
        {
            string[] dirListas = Directory.GetFiles(diretorio, "*.txt", SearchOption.AllDirectories);
            string diretorioDeArcsCriados = $"{AppDomain.CurrentDomain.BaseDirectory}\\Novos Arcs";
            if (!Directory.Exists(diretorioDeArcsCriados))
            {
                Directory.CreateDirectory(diretorioDeArcsCriados);
            }

            foreach (var diretorioLista in dirListas)
            {
                RecriarArcComLista(diretorioLista);
            }

        }

        private void RecriarArcComLista(string dirLista)
        {

            string[] listaDeArquivos = File.ReadAllLines(dirLista);
            string[] split = dirLista.Split(',');
            int tamanhoDoPadding = Convert.ToInt32(split[0].Split('_').Last().Replace(".txt", ""));
            string[] nomeSplitadoArc = Path.GetFileName(dirLista).Split('_');
            string nomeDoArc = string.Join("_", nomeSplitadoArc, 0, nomeSplitadoArc.Length - 1);
            string diretorioBase = dirLista.Replace(Path.GetFileName(dirLista), "");

            CabecalhoArc cabecalhoArc = new() { Assinatura = "ARC\0", Versao = 7, QuantidadeDeArquivos = (ushort)listaDeArquivos.Length };
            int tamanhoAreaEntrada = cabecalhoArc.QuantidadeDeArquivos * 0x90 + 8 + tamanhoDoPadding;
            MemoryStream ms = new MemoryStream();
            int enderecoBase = tamanhoAreaEntrada;

            List<PropriedadesArquivo> propriedadesArquivos = new List<PropriedadesArquivo>();

            using (BinaryWriter bw = new BinaryWriter(ms))
            {

                foreach (var arquivo in listaDeArquivos)
                {
                    bw.BaseStream.Position = enderecoBase;
                    string[] argumentos = arquivo.Split(",");
                    string diretorioArquivo = argumentos[0];
                    bool usarCompresao = bool.Parse(argumentos[1]);
                    uint hashEntesao = Convert.ToUInt32(argumentos[2]);
                    byte[] arquivoEmBytes = File.ReadAllBytes($"{diretorioBase}\\{diretorioArquivo}");
                    int tamanhoArquivo = arquivoEmBytes.Length;


                    if (usarCompresao)
                    {
                        arquivoEmBytes = ComprimirOuDescomprimir(arquivoEmBytes, CompressionMode.Compress);
                    }

                    int tamanhoArquivoComprimido = arquivoEmBytes.Length;



                    PropriedadesArquivo props = new PropriedadesArquivo()
                    {
                        Diretorio = diretorioArquivo.Replace(Path.GetExtension(diretorioArquivo), ""),
                        HashExtensao = hashEntesao,
                        TamanhoComprimido = tamanhoArquivoComprimido,
                        TamanhoDescomprimido = tamanhoArquivo + 0x40000000,
                        Endereco = enderecoBase


                    };

                    propriedadesArquivos.Add(props);
                    bw.Write(arquivoEmBytes);

                    enderecoBase += tamanhoArquivoComprimido;

                }

                bw.BaseStream.Position = 0;

                cabecalhoArc.EscreverPropriedades(bw);

                foreach (var item in propriedadesArquivos)
                {
                    item.EscreverPropriedades(bw);
                }
            }


            File.WriteAllBytes($"{AppDomain.CurrentDomain.BaseDirectory}\\Novos Arcs\\{nomeDoArc}.arc", ms.ToArray());

        }

        public void ExportarArquivos(BinaryReader br, string diretorioArc)
        {

            foreach (var propriedades in PropriedadesDeArquivos)
            {
                br.BaseStream.Position = propriedades.Endereco;
                byte[] arquivo = br.ReadBytes(propriedades.TamanhoComprimido);
                string[] caminhos = propriedades.Diretorio.Split('\\');
                if (!Directory.Exists($"{diretorioArc}\\{string.Join("\\", caminhos, 0, caminhos.Length - 1)}"))
                {
                    Directory.CreateDirectory($"{diretorioArc}\\{string.Join("\\", caminhos, 0, caminhos.Length - 1)}");
                }


                bool temComp = false;

                if (propriedades.TamanhoDescomprimido - 0x40000000 != propriedades.TamanhoComprimido)
                {
                    arquivo = ComprimirOuDescomprimir(arquivo,CompressionMode.Decompress);
                    temComp = true;
                }

                string usaCompresao = temComp == true ? "true" : "false";

                Span<byte> arq = new Span<byte>(arquivo);

                string dirArquivoComExtensao = $"{propriedades.Diretorio}.{ObtenhaExtensao(arq.Slice(0, 4).ToArray())}";

                string infoArquivo = $"{dirArquivoComExtensao},{usaCompresao},{propriedades.HashExtensao}";

                ListaDeCArquivos.Add(infoArquivo);

                File.WriteAllBytes($"{diretorioArc}\\{dirArquivoComExtensao}", arquivo);
            }

            File.WriteAllLines($"{diretorioArc}\\{diretorioArc}_{TamanhoDoPaddingPadding}.txt", ListaDeCArquivos);
        }

        private string ObtenhaExtensao(byte[] extensao)
        {
            return Encoding.ASCII.GetString(extensao).TrimEnd('\0').ToLower();
        }


        public static byte[] ComprimirOuDescomprimir(byte[] bytes, CompressionMode mode)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (ZlibStream zip = new Ionic.Zlib.ZlibStream(ms, mode, true))
                {
                    zip.Write(bytes, 0, bytes.Length);
                }

                return ms.ToArray();
            }
        }

    }


}





