using AutoMapper;
using DevIO.Api.DTOs;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static DevIO.Api.Extensions.CustomAuthorization;

namespace DevIO.Api.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class FornecedoresController : MainController
    {
        private readonly IFornecedorRepository _fornecedorRepository;
        private readonly IFornecedorService _fornecedorService;
        private readonly IEnderecoRepository _enderecoRepository;
        private readonly IMapper _mapper;

        public FornecedoresController(IFornecedorRepository fornecedorRepository,
                                      IFornecedorService fornecedorService,
                                      IMapper mapper,
                                      INotificador notificador, 
                                      IEnderecoRepository enderecoRepository) : base(notificador)
        {
            _fornecedorRepository = fornecedorRepository;
            _fornecedorService = fornecedorService;
            _mapper = mapper;
            _enderecoRepository = enderecoRepository;
        }

        [ClaimsAuthorize("Fornecedor", "Ler")]
        [HttpGet]
        public async Task<IEnumerable<FornecedorDTO>> ObterTodos()
        {
            return _mapper.Map<IEnumerable<FornecedorDTO>>(await _fornecedorRepository.ObterTodos()); 
        }

        [ClaimsAuthorize("Fornecedor", "Ler")]
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FornecedorDTO>> ObterPorId(Guid id)
        {
            var fornecedor = await ObterFornecedorProdutosEndereco(id);

            if (fornecedor == null) return NotFound();

            return Ok(fornecedor);
        }

        [ClaimsAuthorize("Fornecedor", "Adicionar")]
        [HttpPost]
        public async Task<ActionResult<FornecedorDTO>> Adicionar(FornecedorDTO fornecedorDTO)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);
            
            await _fornecedorService.Adicionar(_mapper.Map<Fornecedor>(fornecedorDTO));

            return CustomResponse(fornecedorDTO);
        }

        [ClaimsAuthorize("Fornecedor", "Atualizar")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<FornecedorDTO>> Atualizar(Guid id, FornecedorDTO fornecedorDTO)
        {
            if (id != fornecedorDTO.Id)
            {
                NotificateError("The id passed from query param isn't the same of the id from model");
                return CustomResponse(fornecedorDTO);
            }

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _fornecedorService.Atualizar(_mapper.Map<Fornecedor>(fornecedorDTO));

            return CustomResponse(fornecedorDTO);
        }

        [ClaimsAuthorize("Fornecedor", "Ler")]
        [HttpGet("obter-endereco/{id:guid}")]
        public async Task<EnderecoDTO> ObterEnderecoPorId(Guid id)
        {
            return _mapper.Map<EnderecoDTO>(await _enderecoRepository.ObterPorId(id));
        }

        [ClaimsAuthorize("Fornecedor", "Atualizar")]
        [HttpPut("atualizar-endereco/{id:guid}")]
        public async Task<IActionResult> AtualizarEndereco(Guid id, EnderecoDTO enderecoDTO)
        {
            if (id != enderecoDTO.Id)
            {
                NotificateError("The id passed from query param isn't the same of the id from model");
                return CustomResponse(enderecoDTO);
            }
            
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _fornecedorService.AtualizarEndereco(_mapper.Map<Endereco>(enderecoDTO));

            return CustomResponse(enderecoDTO);
        }

        [ClaimsAuthorize("Fornecedor", "Remover")]
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> Remover(Guid id)
        {
            var fornecedorDto = await ObterFornecedorEndereco(id);

            if(fornecedorDto == null) return NotFound();

            await _fornecedorService.Remover(id);

            return CustomResponse();
        }

        private async Task<FornecedorDTO> ObterFornecedorProdutosEndereco(Guid id)
        {
            return _mapper.Map<FornecedorDTO>(await _fornecedorRepository.ObterFornecedorProdutosEndereco(id));
        }

        private async Task<FornecedorDTO> ObterFornecedorEndereco(Guid id)
        {
            return _mapper.Map<FornecedorDTO>(await _fornecedorRepository.ObterFornecedorEndereco(id));
        }
    }
}
