﻿using System;
using Assets.Game.Blueprints;
using Assets.Game.Components;
using Assets.Game.Configuration;
using EcsRx.Entities;
using EcsRx.Events;
using EcsRx.Extensions;
using EcsRx.Pools;
using EcsRx.Unity;
using EcsRx.Unity.Components;
using UniRx;
using UnityEngine;
using Zenject;

public class AppContainer : EcsRxContainer
{
    private IPool defaultPool;

    [Inject]
    private GameConfiguration _gameConfiguration;

    [Inject]
    private IEventSystem _eventSystem;
    
    protected override void SetupSystems()
    {}

    protected override void SetupEntities()
    {
        defaultPool = PoolManager.GetPool();

        var levelEntity = defaultPool.CreateEntity(new LevelBlueprint());
        var player = defaultPool.CreateEntity(new PlayerBlueprint(_gameConfiguration.StartingFoodPoints));
        var playerView = player.GetComponent<ViewComponent>();
        var levelComponent = levelEntity.GetComponent<LevelComponent>();

        levelComponent.Level.DistinctUntilChanged().Subscribe(x =>
        {
            playerView.View.transform.position = Vector3.zero;
            SetupLevel(levelComponent);
        });

        Observable.Interval(TimeSpan.FromSeconds(1))
            .Subscribe(x => { Debug.Log("current position: " + playerView.View.transform.position); });
    }

    private void SetupLevel(LevelComponent levelComponent)
    {
        levelComponent.HasLoaded.Value = false;

        defaultPool.RemoveEntitiesContaining(typeof(GameBoardComponent),
            typeof(FoodComponent), typeof(WallComponent),
            typeof(EnemyComponent), typeof(ExitComponent));

        Observable.Interval(TimeSpan.FromSeconds(_gameConfiguration.IntroLength))
            .First()
            .Subscribe(x => levelComponent.HasLoaded.Value = true);
            
        defaultPool.CreateEntity(new GameBoardBlueprint());

        for (var i = 0; i < levelComponent.FoodCount; i++)
        { defaultPool.CreateEntity(new FoodBlueprint()); }

        for (var i = 0; i < levelComponent.WallCount; i++)
        { defaultPool.CreateEntity(new WallBlueprint()); }

        for (var i = 0; i < levelComponent.EnemyCount; i++)
        { defaultPool.CreateEntity(new EnemyBlueprint()); }

        defaultPool.CreateEntity(new ExitBlueprint());
    }
}
