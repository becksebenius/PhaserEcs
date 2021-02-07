# Phaser ECS

Phaser is a lightweight ECS system that relies heavily on passing messages between systems to create behavior. Additionally, a GameObjects module is included to facilitate using Unity GameObjects as your ECS entities.

## Core

Phaser is not fundamentally opinionated about the definition of an Entity or Component. The core code formalizes the concept of an Ecs Instance, a System, and Messages.

### Examples

#### Creating an Ecs Instance:

```
public struct FrameUpdateMessage : IMessage
{
	public float DeltaTime;
}

public class GameController : MonoBehaviour
{
	private EcsInstance ecsInstance;

	private void Awake()
	{
		ecsInstance = new EcsInstance(new IEcsSystem[]{
			new System1(),
			new System2()
		});
	}

	private void Update()
	{
		ecsInstance.Update(new FrameUpdateMessage(){
			DeltaTime = Time.deltaTime
		});
	}
}
```

Note that the order that systems are provided to the EcsInstance is also the order that they will process messages.

#### Creating a System

```
public class System1 : SystemBase
{
	public System1()
	{
		AddMessageListener<FrameUpdateMessage>(OnFrameUpdate);
	}

	private void OnFrameUpdate(in FrameUpdateMessage message)
	{
		Debug.Log(message.DeltaTime);
	}
}
```

#### Sending Messages

```
public struct System2FrameUpdatedMessage : IMessage
{
	public string Text;
}

public class System2 : SystemBase
{
	public System2()
	{
		AddMessageListener<FrameUpdateMessage>(OnFrameUpdate);
	}

	private void OnFrameUpdate(in FrameUpdateMessage message)
	{
		// Note - EcsInstance is a property of SystemBase
		EcsInstance.SendMessage(new System2FrameUpdatedMessage(){
			Text = "This message was sent by System2!"
		});
	}
}
```

### Normal vs Deferred Messages

Messages sent using the `SendMessage` API are executed "just-in-time" for each system. This means that the callbacks will be invoked just before the currently processed deferred message callback is executed. Messages are executed in order.

Unlike normal messages, each **Deferred Message** is processed for all systems before moving onto the next deferred message. Typically you would execute a deferred message using the `Update` API, which will queue a deferred message and then execute all pending deferred messages. Deferred messages sent during an Update pass will be executed before control is returned to the Update caller.

## GameObjects

The GameObjects module adds a definition for a GameObjectSystem, which defines a GameObject as an entity, and a Unity Component as a Component.

### GameObjectSystem

GameObjectSystem is a type of System that filters game objects based on the components that they have, and calls callbacks for individual game objects based on messages received.

The generic type parameter for GameObjectSystem is a ComponentSet, which must be a struct. This struct should define a series of Component fields, which the GameObjectSystem will use to determine whether the game object can be acted on by this system. Message Listeners defined on the GameObjectSystem will receive a ComponentSet as a parameter, providing access to the relevant components on the game object.

### Adding GameObjects to an EcsInstance

The GameObjects module provides extension methods to the core EcsInstance that allow you to add game objects to the systems.

```
ecsInstance.AddGameObject(gameObject);
ecsInstance.RemoveGameObject(gameObject);
```

These methods will send GameObjectAddMessage and GameObjectRemovedMessage, respectively, to the ecs instance. These messages are ultimatelly processed by GameObjectSystem systems.

#### EcsHierarchyRoot

You can use the EcsHierarchyRoot component to create a hierarchy of tracked gameobjects in your ecs instance. All immediate children (not recursive) of an Ecs Instance will be automatically tracked by systems when added.

In order to use the EcsHierarchyRoot component, you must also include the provided EcsHierarchySystem system in your system list. It is recommended to put this at/near the top of your systems list so that all subsequent systems can process added/removed game objects.

#### RemoveDestroyedGameObjectsSystem

This system can be used to automatically remove gameobjects from the ecs instance which may have been destroyed by other means.

### Examples

#### Creating a GameObjectSystem

```
public struct MyGameObjectMessage : IGameObjectMessage
{
	public GameObject GameObject { get; set; }
}

public class MyGameObjectSystem : GameObjectSystem<MyGameObjectSystem.ComponentSet>
{
	/*
		The generic parameter for GameObjectSystem must be a struct
		Each field defined in the component set is automatically populated 
			for each GameObject that is considered by this system
		Each field in the component set must be a type that derives from UnityEngine.Component
		Any game object that does not contain all non-optional components as defined in the component set
			will not be added to this system
	*/
	public struct ComponentSet
	{
		public Transform Transform;

		// Optional components will not caused a game object to be excluded from the system if not present
		[Optional] public Rigidbody Rigidbody; 
	}

	public MyGameObjectSystem()
	{
		AddSingleObjectMessageListener<MyGameObjectMessage>(OnMyGameObjectMessageReceived);
		AddPerObjectMessageListener<FrameUpdateMessage>(OnFrameUpdate);
	}

	// This method will be called only for the object that the message was referring to (in the .GameObject property of the message)
	// The message type must be an IGameObjectMessage
	private void OnMyGameObjectMessageReceived(GameObject gameObject, in ComponentSet components, in MyGameObjectMessage message)
	{
		components.Transform.position += Vector3.up; // Example usage of the components parameter
	}

	// This method will be called for all objects tracked by this system
	// The message type does not need to be an IGameObjectMessage
	private void OnMyGameObjectMessageReceived(GameObject gameObject, in ComponentSet components, in MyGameObjectMessage message)
	{
	}
}
```

## Performance

Although I have not benchmarked Phaser, it has been built in such away to specifically avoid runtime memory allocations. Phaser heavily leverages generic type parameters so that all message buffers are allocated upfront. There are some cases where a message buffer might expand, thus causing a new allocation, but these instances should be rare and non-recurring. Future improvements could be made to specify message buffer sizes upfront (perhaps based on past runtime data) but the impact is minimal enough that it's probably not necessary.

On runtime performance, Phaser is *not* designed to be cache-friendly. If you are looking for a cache-friendly solution, you should be looking at Unity's DOTS framework. However, Phaser should still have performance on par with traditionaly gameobject workflows, but with the added benefit of easy-to-use messaging and a clean separation of behavior and state.