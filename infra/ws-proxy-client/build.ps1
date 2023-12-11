$version = [int](Get-Date -UFormat %s -Millisecond 0)
$tag = "ws-proxy-client:$version"

Push-Location .\

Set-Location ..\..\src\WsProxyClient

& docker build -t $tag .

& wsl minikube image load $tag

Pop-Location