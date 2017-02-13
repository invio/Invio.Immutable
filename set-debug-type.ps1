copy 'src\Invio.Immutable\project.json' 'src\Invio.Immutable\project.json.bak'
$project = Get-Content 'src\Invio.Immutable\project.json.bak' -raw | ConvertFrom-Json
$project.buildOptions.debugType = "full"
$project | ConvertTo-Json  | set-content 'src\Invio.Immutable\project.json'