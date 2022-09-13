using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using DevIO.Api.DTOs;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DevIO.Api.Controllers
{
    [Route("api/[controller]")]
    public class ProdutosController : MainController
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IProdutoService _produtoService;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public ProdutosController(INotificador notificador, 
                                IProdutoRepository produtoRepository, 
                                IProdutoService produtoService, 
                                IMapper mapper, 
                                IHttpContextAccessor httpContextAccessor) : base(notificador)
        {
            _produtoRepository = produtoRepository;
            _produtoService = produtoService;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }
        
        [HttpGet]
        public async Task<IEnumerable<ProdutoDTO>> ObterTodos()
        {
            return _mapper.Map<IEnumerable<ProdutoDTO>>(await _produtoRepository.ObterProdutosFornecedores());
        }
        
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutoDTO>> ObterPorId(Guid id)
        {
            var produtoDto = await ObterProduto(id);

            if (produtoDto == null) return NotFound();

            return produtoDto;
        }
        
        [HttpPost]
        public async Task<ActionResult<ProdutoDTO>> Adicionar(ProdutoDTO produtoDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imagemNome = Guid.NewGuid() + "__" + produtoDto.Imagem;
            if (!UploadArquivo(produtoDto.ImagemUpload, imagemNome))
            {
                return CustomResponse(produtoDto);
            }

            produtoDto.Imagem = imagemNome;
            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoDto));

            return CustomResponse(produtoDto);
        }
        
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Atualizar(Guid id, ProdutoDTO produtoDTO)
        {
            if (id != produtoDTO.Id)
            {
                NotificateError("Os ids informados não são iguais!");
                return CustomResponse();
            }

            var produtoAtualizacao = await ObterProduto(id);
  
            if (string.IsNullOrEmpty(produtoDTO.Imagem))
                produtoDTO.Imagem = produtoAtualizacao.Imagem;

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            if (produtoDTO.ImagemUpload != null)
            {
                var imagemNome = Guid.NewGuid() + "_" + produtoDTO.Imagem;
                if (!UploadArquivo(produtoDTO.ImagemUpload, imagemNome))
                {
                    return CustomResponse(ModelState);
                }

                produtoAtualizacao.Imagem = imagemNome;
            }

            produtoAtualizacao.FornecedorId = produtoDTO.FornecedorId;
            produtoAtualizacao.Nome = produtoDTO.Nome;
            produtoAtualizacao.Descricao = produtoDTO.Descricao;
            produtoAtualizacao.Valor = produtoDTO.Valor;
            produtoAtualizacao.Ativo = produtoDTO.Ativo;

            await _produtoService.Atualizar(_mapper.Map<Produto>(produtoAtualizacao));

            return CustomResponse(produtoDTO);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ProdutoDTO>> Remover(Guid id)
        {
            var produto = await ObterProduto(id);
            if (produto == null) return NotFound();

            await _produtoService.Remover(id);
            DeletarArquivo(produto.Imagem);

            return CustomResponse(produto);
        }

        private bool UploadArquivo(string arquivo, string imgNome)
        {
            if (string.IsNullOrEmpty(arquivo))
            {
                NotificateError("Forneça uma imagem para este produto!");
                return false;
            }
            
            var imageDataByteArray = Convert.FromBase64String(arquivo);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/demo-webapi/src/assets", imgNome);

            if (System.IO.File.Exists(filePath))
            {
                NotificateError($"Já existe um arquivo com o nome {imgNome}");
                return false;
            }

            System.IO.File.WriteAllBytes(filePath, imageDataByteArray);
            return true;
        }

        private bool DeletarArquivo(string arquivo)
        {
            if (arquivo == "" || arquivo == null)
            {
                return false;
            }
            
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/demo-webapi/src/assets", arquivo);
            
            if (!System.IO.File.Exists(path))
            {
                NotificateError($"A imagem a ser excluida não existe!");
                return false;
            }
            
            System.IO.File.Delete(path);
            return true;
        }

        public async Task<ProdutoDTO> ObterProduto(Guid id)
        {
            return _mapper.Map<ProdutoDTO>(await _produtoRepository.ObterProdutoFornecedor(id));
        }
    }
}