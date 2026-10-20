$ErrorActionPreference = 'Stop'

$project = Join-Path $PSScriptRoot 'AcademiaDaDuda.Presentation.AppMaui\AcademiaDaDuda.Presentation.AppMaui.csproj'
$androidTemp = Join-Path $PSScriptRoot '.android-temp'

New-Item -ItemType Directory -Force -Path $androidTemp | Out-Null

# O perfil do Windows pode conter caracteres acentuados (por exemplo, Usuário).
# O apksigner do Android falha ao mover arquivos temporários desse caminho.
$env:TEMP = $androidTemp
$env:TMP = $androidTemp

dotnet build $project -f net10.0-android -m:1
exit $LASTEXITCODE
