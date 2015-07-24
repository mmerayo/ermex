## Introduction ##
All ermeX functionallity is in the namespace ermeX.

## Concepts ##
  * **ermeX Component:** components using ermeX.dll
  * **ermeX Network:** a set of ermeX components
---

## How to configure and Start ermeX ##
The configuration can be done programatically. You place the configuration code at your component startup so its introduced to the ermeX Network.

Two steps:
  * Prepare the configuration:
```
     ...
     Configuration cfg = Configuration.Configure(ComponentId).ListeningToTcpPort(6666)
                  .RequestJoinTo(remoteIP,remotePort,remoteComponentId);
     ...
```

  * Start the component using the configuration:
```
    ...
       WorldGate.ConfigureAndStart(cfg); 
    ...
```

#### Mandatory configuration ####
  * Each ermeX component has an unique id in an ermeX network, this is configured as below:
```
   ...
    Configuration cfg = Configuration.Configure(ComponentId);
   ...
```
**ComponentId** should be always the same for a given component

  * Specifying the port where the component listens:
```
   ...
   cfg = cfg.ListeningToTcpPort(myPort);
   ...
```


#### Friend component ####
To join in an ermeX network all your component need to know is another component in the network. As the first component created dont need to join to any component this invocation is not mandatory:
```
   cfg = cfg.RequestJoinTo(remoteIP,remotePort,remoteComponentId)
```

#### Databases ####
ermeX can use the following databases
  * SQL Server: method _SetSqlServerDb(...)_ uses a previously created db (it can be empty) and you pass the connection string to it
  * SQLite: method _SetSqliteDb(...)_ uses an SQLite db
  * In-Memory: method _SetInMemoryDb(...)_ to use only if message delivery between sessions is not mandatory, as the db is removed when the component is disposed

## How to stop the component ##
ermeX dont need to be stopped after is started as it will be implictly done when is disposed, when the host application finishes.

You can stop the component to be reconfigured and restarted by:

```
   WorldGate.Reset();
```

## How to subscribe to a message ##
First you need a subscriptor. This is any type that implements the interface ermeX.IHandleMessages[TheMessageTypeIWantToBeNotified](of.md).

The handler will be invoked everytime a message of the type you specified in the generic parameter is published to the ermeX network


The subscription to messages can be done in two different ways
  * By configuration:
> > Its done when setting up the configuration, you provide the assemblies where the subscriptors are, and specify the types you don't want to subscribe.
```
    ...
    cfg = cfg.DiscoverSubscriptors(assemblies,typesToExclude);
    ...
```
  * Dinamically:
> > Its done once the component was started
```
    ...
    WorldGate.Suscribe<MyHandler>();
    ...
```
_Note that you could subscribe to the same message many times with the same subscriber. Also you could subscribe to base types receiving all the messages that implement the base type_

## How to publish a message ##
When you publish a message to the ermeX network It is received by all the subscribers.
This is done by invoking the following method.
```
   ...
   WorldGate.Publish(message); 
   ...
```


## How to publish one service ##
First you need to create the service interface and the service implementation.

  * The service interface is any that implements ermeX.IService and is decorated as follows.
```
    ...
   [ServiceContract("D6AA6A10-E8BC-4854-AEE8-7AEEDF2C6B1D")]
   public interface IMyService : IService 
   {
       [ServiceOperation("D314C873-2E7A-4C4C-8A2E-06ED6BA49FDB")]
       MyResultType MyMethod(MyBizData theData);
   }
   ...
```
  * The service implementation implements the interface, and its methods will be invoked remotely
```
   public class MyServiceImpl:IMyService
   {
     public MyResultType MyMethod(MyBizData theData)
     {
         MyResultType result;
         result= DoSomething();
         return result;
     }
   }
   ...
```


The subscription to messages can be done in two different ways
  * By configuration:
> > Its done when setting up the configuration, you provide the assemblies where the services are, and specify the types you don't want to publish in those assemblies.
```
    ...
    cfg = cfg.DiscoverServicesToPublish(assemblies,typesToExclude);
    ...
```
  * Dinamically:
> > Its done once the component was started
```
    ...
    WorldGate.RegisterService<IMyService>();
    ...
```
_Note that services which methods dont return anything(void) can be published by several components_

## How to request one service ##
```
   ...
   MyBizData data = new MyBizData(){...} 
   ...
   var myService = WorldGate.GetServiceProxy<IMyService>(); 
   MyResultType result= myService.MyMethod(data);
   ...
```

## How to enable ermeX Logging ##
ermex uses Common.Logging, http://netcommon.sourceforge.net/
The logger name for ermeX logging is _ermeX_