using System;
using System.IO;
using System.Text;
using System.Linq;
using Ionic.Zlib;

namespace TGAARC.Arc
{
    public class PropriedadesArquivo
    {
        public string Diretorio { get; set; }
        public uint HashExtensao { get; set; }
        public string Extensao { get; set; }
        public int TamanhoComprimido { get; set; }
        public int TamanhoDescomprimido { get; set; }
        public int Endereco { get; set; }
        private Guid _GuidArquivoComprimido;

        public PropriedadesArquivo(BinaryReader br)
        {
            Diretorio = Encoding.ASCII.GetString(br.ReadBytes(0x80)).TrimEnd('\0');
            HashExtensao = br.ReadUInt32();
            TamanhoComprimido = br.ReadInt32();
            TamanhoDescomprimido = br.ReadInt32();
            Endereco = br.ReadInt32();
            if (Diretorio.Contains(@"BB\obj\chr\chr230\chr230"))
            {

            }
            string extTemp = ExtensoesArc.Extensoes[HashExtensao];

            if (extTemp.Contains("xfs"))
                Extensao = $".{HashExtensao.ToString("X4")}.{extTemp}";          
            else
                Extensao = extTemp;
            
        }

        public PropriedadesArquivo(string caminho, string caminhoBase, int enderecoAtual) 
        {
            _GuidArquivoComprimido = Guid.NewGuid();
            Endereco = enderecoAtual;
            string extensao = Path.GetExtension(caminho);
            if (extensao.StartsWith(".xfs"))
            {
                string caminhoSemExtesao = caminho.Replace(extensao, "");
                HashExtensao = Convert.ToUInt32(Path.GetExtension(caminhoSemExtesao).Replace(".",""), 16);
                extensao = $".{HashExtensao.ToString("X4")}{extensao}";
            }
            else
                HashExtensao = ExtensoesArc.Extensoes.First(x => x.Value.StartsWith(extensao)).Key;
            
            Diretorio = caminho.Replace($"{caminhoBase}\\","").Replace(Path.GetFileName(caminho), Path.GetFileName(caminho)[5..]).Replace(extensao, "");
            FileInfo informacoesArquivo = new FileInfo(caminho);
            TamanhoDescomprimido = (int)informacoesArquivo.Length + 0x40_00_00_00;
            byte[] arquivoComprimido = Archive.ComprimirOuDescomprimir(File.ReadAllBytes(caminho), CompressionMode.Compress);
            TamanhoComprimido = arquivoComprimido.Length;
            File.WriteAllBytes($"{AppContext.BaseDirectory}\\temp\\{_GuidArquivoComprimido}", arquivoComprimido);

        }

        public void EscreverPropriedades(BinaryWriter bw)
        {
            
            bw.Write(Encoding.ASCII.GetBytes(Diretorio));
            int totalAvancar = 128 - Diretorio.Length;
            bw.BaseStream.Seek(totalAvancar, SeekOrigin.Current);
            bw.Write(HashExtensao);
            bw.Write(TamanhoComprimido);
            bw.Write(TamanhoDescomprimido);
            bw.Write(Endereco);
            bw.BaseStream.Position = Endereco;
            byte[] arquivoComprimido = File.ReadAllBytes($"{AppContext.BaseDirectory}\\temp\\{_GuidArquivoComprimido}");
            bw.Write(arquivoComprimido);           

        }

       
    }
}
