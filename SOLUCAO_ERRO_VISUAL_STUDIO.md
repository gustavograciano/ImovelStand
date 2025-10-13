# 🔧 Solução para Erro no Visual Studio

## ❌ Erro Encontrado

```
Arquivo de origem "C:\Users\gusta\ImovelStand\ImovelStand.Api\obj\Debug\net9.0\ref\ImovelStand.Api.dll" não pode ser encontrado

não foi possível copiar "C:\Users\gusta\ImovelStand\ImovelStand.Api\obj\Debug\net9.0\apphost.exe" para "bin\Debug\net9.0\ImovelStand.Api.exe".
Número de novas tentativas 10 excedido. Falha.
O arquivo é bloqueado por: "ImovelStand.Api (21376)"

Não é possível copiar o arquivo porque ele está sendo usado por outro processo.
```

## 🔍 Causa do Problema

A aplicação **ImovelStand.Api** já está rodando em background (processo PID 21376), impedindo o Visual Studio de compilar e sobrescrever o arquivo executável.

---

## ✅ Solução 1: Matar o Processo via PowerShell (RECOMENDADO)

### Opção A: Script Automático

1. Abra o **PowerShell como Administrador**
2. Navegue até a pasta do projeto:
   ```powershell
   cd C:\Users\gusta\ImovelStand
   ```
3. Execute o script:
   ```powershell
   .\kill-process.ps1
   ```

### Opção B: Comando Manual

Abra o **PowerShell como Administrador** e execute:

```powershell
# Matar todos os processos ImovelStand.Api
Get-Process -Name "ImovelStand.Api" -ErrorAction SilentlyContinue | Stop-Process -Force

# Verificar e matar processos na porta 5082
$pid = (Get-NetTCPConnection -LocalPort 5082 -ErrorAction SilentlyContinue).OwningProcess
if ($pid) {
    Stop-Process -Id $pid -Force
}

Write-Host "Processos finalizados! Agora você pode executar no Visual Studio."
```

---

## ✅ Solução 2: Matar o Processo via Gerenciador de Tarefas

1. Pressione **Ctrl + Shift + Esc** para abrir o Gerenciador de Tarefas
2. Vá para a aba **Detalhes**
3. Procure por **ImovelStand.Api.exe** ou **dotnet.exe** com PID **21376**
4. Clique com o botão direito e selecione **Finalizar Tarefa**
5. Tente compilar novamente no Visual Studio

---

## ✅ Solução 3: Matar o Processo via Prompt de Comando

Abra o **Prompt de Comando como Administrador** e execute:

```cmd
taskkill /F /PID 21376
```

Se não souber o PID, use:

```cmd
taskkill /F /IM ImovelStand.Api.exe
```

---

## ✅ Solução 4: Reiniciar o Visual Studio

1. Feche completamente o Visual Studio
2. Execute a **Solução 1 ou 2** para matar o processo
3. Reabra o Visual Studio
4. Compile o projeto novamente

---

## ✅ Solução 5: Limpar e Reconstruir

Se o problema persistir após matar o processo:

1. No Visual Studio, vá em **Build** → **Clean Solution**
2. Depois vá em **Build** → **Rebuild Solution**

Ou via linha de comando:

```powershell
cd C:\Users\gusta\ImovelStand\ImovelStand.Api
dotnet clean
dotnet build
```

---

## 🚀 Após Resolver

Depois de matar o processo, você pode:

1. **Executar no Visual Studio**: Pressione F5 ou clique em "Start"
2. **Executar via linha de comando**:
   ```powershell
   cd C:\Users\gusta\ImovelStand\ImovelStand.Api
   dotnet run
   ```

A aplicação irá iniciar em: **http://localhost:5082**

---

## 🔍 Verificar se a Porta está Livre

Para verificar se a porta 5082 está sendo usada:

```powershell
# PowerShell
Get-NetTCPConnection -LocalPort 5082 -ErrorAction SilentlyContinue
```

```cmd
# CMD
netstat -ano | findstr :5082
```

Se houver algum processo usando a porta, mate-o antes de executar a aplicação.

---

## 📝 Prevenção Futura

Para evitar este problema:

1. **Sempre pare a aplicação** antes de fechar o terminal ou Visual Studio
2. Use **Ctrl + C** no terminal para parar a aplicação corretamente
3. No Visual Studio, use **Shift + F5** para parar a depuração
4. Verifique se não há processos órfãos rodando antes de compilar novamente

---

## 🆘 Caso o Problema Persista

Se após todas as soluções o erro continuar:

1. **Reinicie o computador** (isso mata todos os processos)
2. **Verifique antivírus**: Às vezes o antivírus bloqueia arquivos
3. **Desative o hot reload**: No Visual Studio, vá em Tools → Options → Debugging → .NET/C++ Hot Reload e desative

---

## ✅ Checklist de Resolução

- [ ] Matar processo via PowerShell (Solução 1)
- [ ] Verificar se a porta 5082 está livre
- [ ] Limpar a solução (dotnet clean)
- [ ] Reconstruir a solução (dotnet build)
- [ ] Executar no Visual Studio (F5)

---

## 🎯 Status Atual

- **Aplicação**: Funcional ✅
- **Banco de Dados**: Conectado ✅
- **Migrations**: Aplicadas ✅
- **Senhas**: Atualizadas ✅
- **Problema**: Processo em background travando compilação

**Após seguir uma das soluções acima, a aplicação deve funcionar perfeitamente no Visual Studio!**

---

**Dica Rápida:**
Execute isto no PowerShell como Admin:
```powershell
Get-Process -Name "ImovelStand.Api" -ErrorAction SilentlyContinue | Stop-Process -Force
```
Depois pressione F5 no Visual Studio! 🚀
