using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.UploadMidiaTemplate
{
    public class UploadMidiaTemplateValidator : AbstractValidator<UploadMidiaTemplateCommand>
    {
        public UploadMidiaTemplateValidator()
        {
            RuleFor(x => x.EmpresaId)
                .NotEmpty().WithMessage("Não foi possível identificar a empresa.");

            RuleFor(x => x.Arquivo)
                .NotEmpty().WithMessage("Nenhum arquivo enviado.");

            RuleFor(x => x.MimeType)
                .NotEmpty().WithMessage("Não foi possível identificar o tipo do arquivo.");
        }
    }
}
