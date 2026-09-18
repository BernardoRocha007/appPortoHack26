namespace appPortoHack.API.Models
{
    //Representa a regra de NCM, que é o conjunto de atributos obrigatórios para um determinado NCM
    public class RegraNcm
    {
        public string Ncm { get; set; } = string.Empty;

        public string Descricao {get ; set;} = string.Empty;

        public string OrgaoAnuente {get ; set;} = string.Empty;
        public bool RequerLpco { get; set; }
        public List<AtributoObrigatorio> AtributosObrigatorios { get; set; } = new();
    }
// Representa cada campo exigido pelo governo
public class AtributoObrigatorio
{
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}

}