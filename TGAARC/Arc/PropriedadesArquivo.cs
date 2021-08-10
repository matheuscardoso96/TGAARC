using System.IO;
using System.Text;

namespace TGAARC.Arc
{
    public class PropriedadesArquivo
    {
        public string Diretorio { get; set; }
        public uint HashExtensao { get; set; }
        public int TamanhoComprimido { get; set; }
        public int TamanhoDescomprimido { get; set; }
        public int Endereco { get; set; }

        public PropriedadesArquivo(BinaryReader br)
        {
            Diretorio = Encoding.ASCII.GetString(br.ReadBytes(0x80)).TrimEnd('\0');
            HashExtensao = br.ReadUInt32();
            TamanhoComprimido = br.ReadInt32();
            TamanhoDescomprimido = br.ReadInt32();
            Endereco = br.ReadInt32();
        }

        public PropriedadesArquivo() { }

        public void EscreverPropriedades(BinaryWriter bw)
        {
            bw.Write(Encoding.ASCII.GetBytes(Diretorio));
            int totalAvancar = 128 - Diretorio.Length;
            bw.BaseStream.Seek(totalAvancar, SeekOrigin.Current);
            bw.Write(HashExtensao);
            bw.Write(TamanhoComprimido);
            bw.Write(TamanhoDescomprimido);
            bw.Write(Endereco);


        }
    }
}
