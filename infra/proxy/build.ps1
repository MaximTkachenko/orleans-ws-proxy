$version = [int](Get-Date -UFormat %s -Millisecond 0)
$tag = "ws-proxy:$version"

Push-Location .\

Set-Location ..\..\src\WsProxy

& docker build -t $tag .

& wsl minikube image load $tag

Pop-Location

$tag