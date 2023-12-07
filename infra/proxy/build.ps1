$version = [int](Get-Date -UFormat %s -Millisecond 0)
$tag = "ws-proxy:$version"

& cd ..\..\src

& docker build -t $tag .

& wsl minikube image load $tag