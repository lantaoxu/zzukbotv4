﻿using Harvester.Engine.Loaders;
using System;
using ZzukBot.Core.Game.Statics;
using ZzukBot.Core.Mem;

namespace Harvester.Engine
{
    public class Manager
    {
        CCLoader CCLoader { get; }
        Controller Controller { get; }
        ObjectManager ObjectManager { get; }
        ProfileLoader ProfileLoader { get; }
        Action stopCallback;
        readonly MainThread.Updater MainThread;
        readonly object obj;
        bool running;

        public Manager(CCLoader ccLoader, Controller controller, ObjectManager objectManager,
            ProfileLoader profileLoader)
        {
            CCLoader = ccLoader;
            Controller = controller;
            ObjectManager = objectManager;
            ProfileLoader = profileLoader;
            MainThread = new MainThread.Updater(Pulse, 250);
            obj = new object();
        }

        public bool Start(Action stopCallback)
        {
            lock (obj)
            {
                if (running) return false;
                if (!ObjectManager.IsIngame) return false;
                if (ObjectManager.Player == null) return false;
                try { if (ProfileLoader.Hotspots == null) return false; } catch { return false; }
                Console.WriteLine("Loading custom class");
                if (!CCLoader.LoadCustomClass(ObjectManager.Player.Class)) return false;
                running = true;
            }
            this.stopCallback = stopCallback;
            MainThread.Start();

            return running;
        }

        public void Stop() => running = false;
        public void Dispose() { }
        public void Pause() { }
        public bool Resume() => true;

        void Pulse()
        {
            Console.WriteLine("pulse");
            ObjectManager.Player.AntiAfk();
            Controller.Behavior();
            if (running) return;
            MainThread.Stop();
            stopCallback();
        }
    }
}
