﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCubosAlberto.Models
{
    [Table("COMPRACUBOS")]
    public class CompraCubos
    {
        [Key]
        [Column("id_pedido")]
        public int IdPedido { get; set; }

        [Column("id_cubo")]
        public int IdCubo { get; set; }

        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Column("fecha")]
        public DateTime fecha { get; set; }
    }
}
