using ProcessoSeletivo.Models;
using ProcessoSeletivo.Resources;
using ProcessoSeletivo.VO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace ProcessoSeletivo.Controllers
{
    public class CadastroDesenvolvedorController : Controller
    {
        public ActionResult Index()
        {
            var view = View();

            CarregarTextosAPartirDosArquivosDeRecurso(view);

            return view;
        }

        /// <summary>
        /// Utilizamos os arquivos de recursos (.resx) para facilitar a manutenção 
        /// e também a globalização do sistema como um todo.
        /// </summary>
        private static void CarregarTextosAPartirDosArquivosDeRecurso(ViewResult view)
        {
            view.ViewBag.Saudacao = Label.SaudacaoPagina;
            view.ViewBag.LabelNome = Label.Nome;
            view.ViewBag.LabelEmail = Label.Email;
            view.ViewBag.CampoObrigatorioNome = Mensagem.CampoObrigatorioNome;
            view.ViewBag.CampoObrigatorioEmail = Mensagem.CampoObrigatorioEmail;
            view.ViewBag.EmailInvalido = Mensagem.EmailInvalido;
            view.ViewBag.DesenvolvimentoIOS = Label.DesenvolvimentoIOS;
            view.ViewBag.DesenvolvimentoAndroid = Label.DesenvolvimentoAndroid;
            view.ViewBag.DescricaoDoCadastro = Label.DescricaoDoCadastro;
            view.ViewBag.SaudacaoAposDescricaoDoCadastro = Label.SaudacaoAposDescricaoDoCadastro;
            view.ViewBag.InstrucoesParaDadosBasicos = Label.InstrucoesParaDadosBasicos;
            view.ViewBag.InstrucoesParaAvaliacaoConhecimentos = Label.InstrucoesParaAvaliacaoConhecimentos;
            view.ViewBag.AgradecimentoPeloCadastro = Label.AgradecimentoPeloCadastro;
        }

        /// <summary>
        /// Método post que recebe os dados do formulário preenchido e realiza a chamada de cada método para o processamento.
        /// </summary>
        [HttpPost]
        public JsonResult EnviarDadosDoCadastro(FormCollection formCollection)
        {
            //Resposta que será enviada como mensagem para uma confirmação do envio ou uma falha no envio.
            String resposta = Mensagem.CadastroEnviadoComSucesso;

            try
            {
                var candidato = PreencherModelCandidato(formCollection);

                PersistirDadosCandidato(candidato);

                resposta = ProcessarRespostaParaOCanditadoDeAcordoComSeusConhecimentos(candidato);
            }
            catch (Exception ex)
            {
                resposta = String.Format("{0}:{1}", Mensagem.ErroAoEnviar, ex.Message);
                throw;
            }

            return Json(resposta, JsonRequestBehavior.AllowGet);
        }

        private static Candidato PreencherModelCandidato(FormCollection formCollection)
        {
            return new Candidato()
            {
                Nome = formCollection.Get("txtNome"),
                Email = formCollection.Get("txtEmail"),
                ConhecimentosTecnicos = new List<ConhecimentosVO>()
                    {
                        new ConhecimentosVO () { Descricao = "HTML", EscalaDe0ate10 = formCollection.Get("rngHtml") },
                        new ConhecimentosVO () { Descricao = "CSS", EscalaDe0ate10 = formCollection.Get("rngCSS") },
                        new ConhecimentosVO () { Descricao = "Javascript", EscalaDe0ate10 = formCollection.Get("rngJavascript") },
                        new ConhecimentosVO () { Descricao = "Python", EscalaDe0ate10 = formCollection.Get("rngPython") },
                        new ConhecimentosVO () { Descricao = "Django", EscalaDe0ate10 = formCollection.Get("rngDjango") },
                        new ConhecimentosVO () { Descricao = "iOS", EscalaDe0ate10 = formCollection.Get("rngiOS") },
                        new ConhecimentosVO () { Descricao = "Android", EscalaDe0ate10 = formCollection.Get("rngAndroid") }
                    }
            };
        }

        /// <summary>
        /// Método responsável para salvar os dados do candidato no banco de dados
        /// </summary>
        /// <param name="candidato">Model candidato devidamente preenchida</param>
        private void PersistirDadosCandidato(Candidato candidato)
        {
            //Implementação para salvar no banco os dados do candidato
            //porém acredito ser desnecessário para o teste proposto
        }

        /// <summary>
        /// Método responsável de tratar a regra de resposta de acordo com a escala do candidato em cada grupo de conhecimento
        /// </summary>
        /// <param name="candidato">Model candidato devidamente preenchida</param>
        /// <returns>Retorna o que seria o e-mail para enviar por alert 
        /// (posteriormente irá virar void após implementar o envio de email)</returns>
        private String ProcessarRespostaParaOCanditadoDeAcordoComSeusConhecimentos(Candidato candidato)
        {
            var escalaHTML = Convert.ToInt32(candidato.ConhecimentosTecnicos.Where(i => i.Descricao == "HTML").FirstOrDefault().EscalaDe0ate10);
            var escalaCSS = Convert.ToInt32(candidato.ConhecimentosTecnicos.Where(i => i.Descricao == "CSS").FirstOrDefault().EscalaDe0ate10);
            var escalaJavascript = Convert.ToInt32(candidato.ConhecimentosTecnicos.Where(i => i.Descricao == "Javascript").FirstOrDefault().EscalaDe0ate10);
            var escalaPython = Convert.ToInt32(candidato.ConhecimentosTecnicos.Where(i => i.Descricao == "Python").FirstOrDefault().EscalaDe0ate10);
            var escalaDjango = Convert.ToInt32(candidato.ConhecimentosTecnicos.Where(i => i.Descricao == "Django").FirstOrDefault().EscalaDe0ate10);
            var escalaiOS = Convert.ToInt32(candidato.ConhecimentosTecnicos.Where(i => i.Descricao == "iOS").FirstOrDefault().EscalaDe0ate10);
            var escalaAndroid = Convert.ToInt32(candidato.ConhecimentosTecnicos.Where(i => i.Descricao == "Android").FirstOrDefault().EscalaDe0ate10);

            var oCandidatoSeEnquadrouEmAlgumPerfil = false;

            const Int32 ESCALA_MINIMA_CONHECIMENTO = 7;
            const String PULAR_LINHA = "\n\n";

            var respostaPorAlertaAoInvesDeEmail = "";

            if (escalaHTML > ESCALA_MINIMA_CONHECIMENTO &&
                escalaCSS > ESCALA_MINIMA_CONHECIMENTO &&
                escalaJavascript > ESCALA_MINIMA_CONHECIMENTO)
            {
                respostaPorAlertaAoInvesDeEmail = EnviarEmailFrontEnd(candidato);
                oCandidatoSeEnquadrouEmAlgumPerfil = true;
            }

            if (escalaPython > ESCALA_MINIMA_CONHECIMENTO &&
               escalaDjango > ESCALA_MINIMA_CONHECIMENTO)
            {
                respostaPorAlertaAoInvesDeEmail = String.Concat(respostaPorAlertaAoInvesDeEmail,
                                                                PULAR_LINHA,
                                                                EnviarEmailBackEnd(candidato));
                oCandidatoSeEnquadrouEmAlgumPerfil = true;
            }

            if (escalaiOS > ESCALA_MINIMA_CONHECIMENTO &&
               escalaAndroid > ESCALA_MINIMA_CONHECIMENTO)
            {
                respostaPorAlertaAoInvesDeEmail = String.Concat(respostaPorAlertaAoInvesDeEmail,
                                                                PULAR_LINHA,
                                                                EnviarEmailMobile(candidato));
                oCandidatoSeEnquadrouEmAlgumPerfil = true;
            }

            if (!oCandidatoSeEnquadrouEmAlgumPerfil)
            {
                respostaPorAlertaAoInvesDeEmail = String.Concat(respostaPorAlertaAoInvesDeEmail,
                                                                PULAR_LINHA,
                                                                EnviarEmailGenerico(candidato));
            }

            return respostaPorAlertaAoInvesDeEmail;
        }

        /// <summary>
        /// Envia o email caso o perfil do candidato se encaixe em FrontEnd.
        /// </summary>
        /// <param name="candidato">Model candidato devidamente preenchida</param>
        /// <returns>Retorna o que seria o e-mail para enviar por alert 
        /// (posteriormente irá virar void após implementar o envio de email)</returns>
        private String EnviarEmailFrontEnd(Candidato candidato)
        {
            var destinatario = candidato.Email;
            var assunto = Mensagem.AssuntoEmail;
            var corpoDoEmail = Mensagem.CorpoEmailFrontEnd;

            //Essa parte é só pra enviar alerta ao invés de email
            var formatacaoParaEnviarAlertaAoInvesDeEmail = String.Format("Assunto: {0} \nConteúdo: {1}", assunto, corpoDoEmail);
            return formatacaoParaEnviarAlertaAoInvesDeEmail;
        }

        /// <summary>
        /// Envia o email caso o perfil do candidato se encaixe em BackEnd.
        /// </summary>
        /// <param name="candidato">Model candidato devidamente preenchida</param>
        /// <returns>Retorna o que seria o e-mail para enviar por alert 
        /// (posteriormente irá virar void após implementar o envio de email)</returns>
        private String EnviarEmailBackEnd(Candidato candidato)
        {
            var destinatario = candidato.Email;
            var assunto = Mensagem.AssuntoEmail;
            var corpoDoEmail = Mensagem.CorpoEmailBackEnd;

            //Essa parte é só pra enviar alerta ao invés de email
            var formatacaoParaEnviarAlertaAoInvesDeEmail = String.Format("Assunto: {0} \nConteúdo: {1}", assunto, corpoDoEmail);
            return formatacaoParaEnviarAlertaAoInvesDeEmail;
        }

        /// <summary>
        /// Envia o email caso o perfil do candidato se encaixe em Mobile.
        /// </summary>
        /// <param name="candidato">Model candidato devidamente preenchida</param>
        /// <returns>Retorna o que seria o e-mail para enviar por alert 
        /// (posteriormente irá virar void após implementar o envio de email)</returns>
        private String EnviarEmailMobile(Candidato candidato)
        {
            var destinatario = candidato.Email;
            var assunto = Mensagem.AssuntoEmail;
            var corpoDoEmail = Mensagem.CorpoEmailMobile;

            //Essa parte é só pra enviar alerta ao invés de email
            var formatacaoParaEnviarAlertaAoInvesDeEmail = String.Format("Assunto: {0} \nConteúdo: {1}", assunto, corpoDoEmail);
            return formatacaoParaEnviarAlertaAoInvesDeEmail;
        }

        /// <summary>
        /// Envia o email caso o candidato não se encaixe em nenhum perfil.
        /// </summary>
        /// <param name="candidato">Model candidato devidamente preenchida</param>
        /// <returns>Retorna o que seria o e-mail para enviar por alert 
        /// (posteriormente irá virar void após implementar o envio de email)</returns>
        private String EnviarEmailGenerico(Candidato candidato)
        {
            var destinatario = candidato.Email;
            var assunto = Mensagem.AssuntoEmail;
            var corpoDoEmail = Mensagem.CorpoEmailGenerico;

            //Essa parte é só pra enviar alerta ao invés de email
            var formatacaoParaEnviarAlertaAoInvesDeEmail = String.Format("Assunto: {0} \nConteúdo: {1}", assunto, corpoDoEmail);
            return formatacaoParaEnviarAlertaAoInvesDeEmail;
        }
    }
}
