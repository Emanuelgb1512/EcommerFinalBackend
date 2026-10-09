namespace EcommerceAPI.Models
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Cliente = "Cliente";
    }

    public static class EstadosPedido
    {
        public const string Pendiente = "Pendiente";
        public const string Pagado = "Pagado";
        public const string Enviado = "Enviado";
        public const string Entregado = "Entregado";
        public const string Cancelado = "Cancelado";

        public static readonly string[] Todos = { Pendiente, Pagado, Enviado, Entregado, Cancelado };
    }

    public static class EstadosPago
    {
        public const string Pendiente = "Pendiente";
        public const string EnProceso = "En proceso";
        public const string Aprobado = "Aprobado";
        public const string Rechazado = "Rechazado";
        public const string Reembolsar = "Reembolsar";
        public const string Cancelado = "Cancelado";

        // Los que el administrador puede elegir a mano
        public static readonly string[] Editables = { Pendiente, EnProceso, Aprobado, Rechazado };
    }

    public static class Tarifas
    {
        public const decimal Iva = 0.19m;   // 19 %
    }
}