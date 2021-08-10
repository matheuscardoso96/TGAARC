using System.IO;
using System.Text;

namespace TGAARC.Arc
{
    public class CabecalhoArc
    {
        public string Assinatura { get; set; }
        public ushort Versao { get; set; }
        public ushort QuantidadeDeArquivos { get; set; }

        public CabecalhoArc(BinaryReader br)
        {
            Assinatura = Encoding.ASCII.GetString(br.ReadBytes(4));
            Versao = br.ReadUInt16();
            QuantidadeDeArquivos = br.ReadUInt16();
        }
        public CabecalhoArc() { }

        public void EscreverPropriedades(BinaryWriter bw)
        {
            bw.Write(Encoding.ASCII.GetBytes(Assinatura));
            bw.Write(Versao);
            bw.Write(QuantidadeDeArquivos);

        }

    }
}
