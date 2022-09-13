using AutoMapper;
using DevIO.Api.DTOs;
using DevIO.Business.Models;

namespace DevIO.Api.Configuration
{
    public class AutoMapperConfig : Profile
    {
        public AutoMapperConfig()
        {
            CreateMap<Fornecedor, FornecedorDTO>().ReverseMap();
            CreateMap<Endereco, EnderecoDTO>().ReverseMap();
            CreateMap<ProdutoDTO, Produto>();

            CreateMap<ProdutoImageDTO, Produto>().ReverseMap();

            CreateMap<Produto, ProdutoDTO>().ForMember(dest =>
                dest.NomeFornecedor, opt => opt.MapFrom(src => src.Fornecedor.Nome));
        }
    }
}
