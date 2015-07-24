### ermeX is recruiting now!! ###
Would you like to contribute to ermeX?
Please apply here https://groups.google.com/forum/?fromgroups=#!topic/forum-ermex/j0PXlxJzdqU

or send the project owner an e-mail ( mmerayo30@gmail.com ) explaining what you can do for ermeX.

## Any question? ##
Send it here  mmerayo30@gmail.com


## What is it? ##
ermeX is a fast library implementing a _Service Bus_ with a minimal usage complexity.

A set of connected ermeX components is an _ermeX Network_

## Quick starts ##
The usage of ermeX is really easy, we recommend to start by viewing the Quick Start alarms sample which you can download in the downloads section, it should take you just 15 minutes to be ready to use ermeX.

http://letsermex.blogspot.com.es/

## NuGet it ##
https://nuget.org/packages?q=ermeX

## What does it do? ##
![http://ermex.googlecode.com/svn/wiki/diagram.jpg](http://ermex.googlecode.com/svn/wiki/diagram.jpg)
  * ermeX allows you to request services and subscribe to messages without having configuration nightmares
  * With ermeX you can call remote objects like if they were local and you can implement distributed message queues in the simplest way.
  * To deploy ermeX you just need the dll referenced in your project and you dont need any third party tool like MSMQ, databases installed... and the message delivery is guaranteed
  * All ermeX functionallity is achieved from only one static class, called **WorldGate** .


## How to use it? ##
Please, take a look at the QuickStart
  * Reference the ermeX.dll (or nuGet it!!)
  * Configure ermeX on your component startup with the ip and port of another component using ermeX and your listening port
```
   Configuration cfg = Configuration.Configure(ComponentId).ListeningToTcpPort(6666)
                      .RequestJoinTo(remoteIP,remotePort,remoteComponentId);
   WorldGate.ConfigureAndStart(cfg); //after this you are in the ermeX network
```

  * Publish your messages to the ermeX Network
```
   ...
   WorldGate.Publish(message); //message can be any object
   ...
```
  * Subscribe to messages: the subscription to messages can be discovered automatically by configuration(recommended), or done explicitly returning the handler instance for example:
```
   //first declare the handler
   //AlarmsMessagesHandler implements the key interface IHandleMessages<AlarmMessage>.  
   public class AlarmsMessagesHandler: IHandleMessages<AlarmMessage>   
   {...}
   ...
   //after the following line, your ermeX component will handle all the 
   //messages of type AlarmsMessage and inheritors published in its ermeX Network  
   var handler = WorldGate.Suscribe<AlarmsMessagesHandler>(); //this call returns the instance that will handle the messages. 
   //The preferred option is the auto-discovery by config
   ...
```
  * Publish your services: same as the messages subscriptions, the services can be auto discovered by configuration(recommended), or published explicitly.
Note that the interface must be in a referenced assembly when needed by the caller. this and the attributes will be simplified in the future, keeping the backwards compatibility
```
   ...
   //First create your service interface 
   [ServiceContract("D6AA6A10-E8BC-4854-AEE8-7AEEDF2C6B1D")]
   public interface IMyService : IService //an ermeX service Implements the empty interface IService
   {
       [ServiceOperation("D314C873-2E7A-4C4C-8A2E-06ED6BA49FDB")]
       MyResultType MyMethod(MyBizData theData);
   }
   ...
   //Now implement your interface
   public class MyServiceImpl:IMyService
   {...}
   ...
   //Lets publish MyService explictly here. 
   WorldGate.RegisterService(typeof (MyService));//Auto discovery is the preferred option
   ...
```

  * Call services: as mentioned before, you need the assembly containing the service interface referenced by your component
```
  ...
  MyBizData data = new MyBizData(){...} //in this case the service has a parameter  
  ...
  var myService = WorldGate.GetServiceProxy<IMyService>(); //first we get the proxy
  MyResultType result= myService.MyMethod(data);  //then we invoke the service. we dont need to know who provides it as it can be published by any component in the ermeX Network
  ...
```


## Goals ##
  1. **Multiplatform:**  Not only for .net(MS and MONO). ermeX roadmap will bring ermeX available through most of the development platforms and programming languages
  1. **Self contained:** No need of pre-requirements installed, just reference it in your project and use it
  1. **Easy to use:** Simplest configuration, simplest usage
  1. **Fast:** one of ermeX Devs main goals is the performance
  1. **Continuous improvement**



## Features ##

The current version covers the following features:
  1. Subscribe to messages
  1. Publish messages to subscribers
  1. Publish services
  1. Request services

For detailed instructions about the usage please see the _Wiki_ section.


![http://ermex.googlecode.com/svn/wiki/Network.png](http://ermex.googlecode.com/svn/wiki/Network.png)

---

Suggestions, questions and development help are very welcomed.