namespace AnlUsuarui.Models
{
    public class UsuarioModel
    {
        public Guid Id { get; set; }
        public string Nome { get; set; } = "";
        public int Idade { get; set; }
        public int Score { get; set; }
        public bool Ativo { get; set; }
        public string Pais { get; set; } = "";
        public TeamModel Equipe { get; set; }
        public IEnumerable<LogModel> Logs { get; set; }
    }

    public class TeamModel
    {
        public string Nome { get; set; } = "";
        public bool Lider { get; set; }
        public IEnumerable<ProjectModel> Projetos { get; set; }
    }
    public class ProjectModel
    {
        public string Nome { get; set; }
        public bool Concluido { get; set; }
    }
    public class LogModel
    {
        public DateTime Data { get; set; }
        public string Acao { get; set; } = "";
    }
}