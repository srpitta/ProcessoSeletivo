using ProcessoSeletivo.VO;
using System;
using System.Collections.Generic;

namespace ProcessoSeletivo.Models
{
    public class Candidato : Pessoa
    {
        public String Email { get; set; }
        public List<ConhecimentosVO> ConhecimentosTecnicos { get; set; }
    }
}