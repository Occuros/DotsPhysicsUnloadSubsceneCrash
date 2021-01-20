using Unity.Collections;
using Unity.Entities;
using Unity.Physics.Systems;
using Unity.Scenes;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Unity.Physics.Stateful
{
	// [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
	// [UpdateBefore(typeof(BuildPhysicsWorld))]
	public class ReloadSubSceneSystem: SystemBase
	{

		private bool _sceneUnloaded;

		protected override void OnCreate()
		{
		}

		protected override void OnUpdate()
		{

			var shouldReload = new NativeArray<bool>(1, Allocator.TempJob);
			var sceneSystem = World.GetOrCreateSystem<SceneSystem>();

			var spacePressed = Input.GetKeyDown(KeyCode.Space);

			Entities
				.ForEach((Entity entity, DynamicBuffer<StatefulCollisionEvent> collisionEvents) =>
			{
				for (var index = 0; index < collisionEvents.Length; index++)
				{
					var collisionEvent = collisionEvents[index];
					if (collisionEvent.CollidingState == EventCollidingState.Enter)
					{
						Debug.Log("Collision Enter Event");

						shouldReload[0] = true;
					}
				}
			}).Run();
			
			Entities
				.WithoutBurst()
				.WithStructuralChanges()
				.ForEach((SubScene subScene) =>
			{
				if (spacePressed && _sceneUnloaded)
				{
					Debug.Log("Loading Scene");
					sceneSystem.LoadSceneAsync(subScene.SceneGUID);
					_sceneUnloaded = false;

				}
				
				if (shouldReload[0])
				{
					Debug.Log("Unloading Scene");
					sceneSystem.UnloadScene(subScene.SceneGUID);
					_sceneUnloaded = true;
				}
				
			}).Run();

			shouldReload.Dispose(Dependency);
		}
	}
}