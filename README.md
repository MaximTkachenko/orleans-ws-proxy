# orleans-ws-proxy

- [install minikube](https://minikube.sigs.k8s.io/docs/start/)
- deploy:
    - azurite
    - ws-echo-server
    - proxy
    - ws-proxy-client
- [orleans dahsboard](http://localhost:5223/)
- [ws-proxy-client swagger](http://localhost:8082/swagger/index.html)
- [presentation](./presentation/)

![architecture diagram](architecture-diagram.svg)


TODO Slides:
- distributed systems intro: stateless, stateful, data + behavior
- describe orleans approach, main ideas liek: grain, silo, location transparancy, cluster provider, placement, activation, single threaded execution, state (cheap caching)
- examples: proxy, loadbalancing, ocppbridge
- architecture
- minikube demo
- show the code
