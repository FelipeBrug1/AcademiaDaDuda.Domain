using AcademiaDaDuda.Application.DTOs;
using AcademiaDaDuda.Application.Interfaces;
using AcademiaDaDuda.Application.Mappings;
using AcademiaDaDuda.Domain.Repositories;
using AcademiaDaDuda.Domain.ValueObjects;

namespace AcademiaDaDuda.Application.Services;

public class LogradouroService : ILogradouroService
{
    private readonly Func<ILogradouroRepository> _repoFactory;

    public LogradouroService(Func<ILogradouroRepository> repoFactory)
    {
        _repoFactory = repoFactory ?? throw new ArgumentNullException(nameof(repoFactory));
    }

    public async Task<bool> CepJaExisteAsync(string cep, int? id = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cep))
            return false;

        var cepResult = Cep.Criar(cep);
        if (cepResult.IsFailure)
            return false;

        return await UseRepositoryAsync(repository =>
            repository.CepJaExiste(cepResult.Value!, id, cancellationToken));
    }

    public async Task<LogradouroDto?> ObterPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        await UseRepositoryAsync(async repository =>
            (await repository.ObterPorId(id, cancellationToken))?.ToDto());

    public async Task<IEnumerable<LogradouroDto>> ObterTodosAsync(CancellationToken cancellationToken = default) =>
        await UseRepositoryAsync(async repository =>
        {
            var items = await repository.ObterTodos(cancellationToken);
            return items.Select(item => item.ToDto()).ToList();
        });

    public async Task<LogradouroDto?> ObterPorCepAsync(string cep, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cep))
            throw new ArgumentException("CEP não pode ser vazio.", nameof(cep));

        var cepResult = Cep.Criar(cep);
        if (cepResult.IsFailure)
            throw new ArgumentException($"CEP inválido: {string.Join(", ", cepResult.Notifications.Select(n => n.Mensagem))}", nameof(cep));

        return await UseRepositoryAsync(async repository =>
            (await repository.ObterPorCep(cepResult.Value!, cancellationToken))?.ToDto());
    }

    public async Task<IEnumerable<LogradouroDto>> ObterPorCidadeAsync(string cidade, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cidade))
            throw new ArgumentException("Cidade não pode ser vazia.", nameof(cidade));

        return await UseRepositoryAsync(async repository =>
        {
            var items = await repository.ObterPorCidade(cidade.Trim(), cancellationToken);
            return items.Select(item => item.ToDto()).ToList();
        });
    }

    public async Task<IEnumerable<LogradouroDto>> ObterPorBairroAsync(string cidade, string bairro, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cidade))
            throw new ArgumentException("Cidade não pode ser vazia.", nameof(cidade));
        if (string.IsNullOrWhiteSpace(bairro))
            throw new ArgumentException("Bairro não pode ser vazio.", nameof(bairro));

        return await UseRepositoryAsync(async repository =>
        {
            var items = await repository.ObterPorBairro(cidade.Trim(), bairro.Trim(), cancellationToken);
            return items.Select(item => item.ToDto()).ToList();
        });
    }

    public async Task<LogradouroDto> AdicionarAsync(LogradouroDto logradouroDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(logradouroDto);
        var cepResult = Cep.Criar(logradouroDto.Cep);
        if (cepResult.IsFailure)
            throw new ArgumentException($"CEP inválido: {string.Join(", ", cepResult.Notifications.Select(n => n.Mensagem))}", nameof(logradouroDto));

        var entity = logradouroDto.ToEntity();
        return await UseRepositoryAsync(async repository =>
        {
            if (await repository.CepJaExiste(cepResult.Value!, null, cancellationToken))
                throw new InvalidOperationException($"Já existe um logradouro cadastrado com o CEP {logradouroDto.Cep}.");

            return (await repository.Adicionar(entity, cancellationToken)).ToDto();
        });
    }

    public async Task<LogradouroDto> AtualizarAsync(LogradouroDto logradouroDto, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(logradouroDto);
        var cepResult = Cep.Criar(logradouroDto.Cep);
        if (cepResult.IsFailure)
            throw new ArgumentException($"CEP inválido: {string.Join(", ", cepResult.Notifications.Select(n => n.Mensagem))}", nameof(logradouroDto));

        return await UseRepositoryAsync(async repository =>
        {
            var current = await repository.ObterPorId(logradouroDto.Id, cancellationToken)
                ?? throw new KeyNotFoundException($"Logradouro com ID {logradouroDto.Id} não encontrado.");

            if (await repository.CepJaExiste(cepResult.Value!, logradouroDto.Id, cancellationToken))
                throw new InvalidOperationException($"Já existe outro logradouro cadastrado com o CEP {logradouroDto.Cep}.");

            return (await repository.Atualizar(current.UpdateFromDto(logradouroDto), cancellationToken)).ToDto();
        });
    }

    public async Task<bool> RemoverAsync(int id, CancellationToken cancellationToken = default) =>
        await UseRepositoryAsync(async repository =>
        {
            if (await repository.ObterPorId(id, cancellationToken) is null)
                return false;
            return await repository.Remover(id, cancellationToken);
        });

    private Task<TResult> UseRepositoryAsync<TResult>(Func<ILogradouroRepository, Task<TResult>> operation) =>
        RepositoryExecution.ExecuteAsync(_repoFactory, operation);
}
