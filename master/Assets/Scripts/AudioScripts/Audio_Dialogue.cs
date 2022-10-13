// This code is part of the Fungus library (https://github.com/snozbot/fungus)
// It is released for free under the MIT open source license (https://github.com/snozbot/fungus/blob/master/LICENSE)

// Neil 's modified version of the script to use FMOD

﻿using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UIElements;

namespace Fungus
{
    public class Audio_Dialogue : MonoBehaviour
    {
        [SerializeField] protected FMODUnity.EventReference dialEventRef;

        protected FMOD.Studio.EventInstance dialEventInstance;

        protected bool dialPaused;

        [SerializeField] GameObject playerGO;

        private GameObject dialGO;


        protected virtual void Awake()
        {
            WriterSignals.OnWriterGlyph += GlyphWritten;
            BlockSignals.OnBlockEnd += OnBlockEnd;
            EventListener.Get(playerGO).OnTriggerEnterDelegate2 += StoryTellerTrigger;

            dialGO = playerGO;
        }

        private void GlyphWritten(object sender)
        {
            dialEventInstance = FMODUnity.RuntimeManager.CreateInstance(dialEventRef);
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(dialEventInstance, dialGO.transform);
            dialEventInstance.start();
        }

        private void StoryTellerTrigger(GameObject a, GameObject b)
        {
            //Debug.Log("b: " + b.transform.parent.name);

            if (a.name.ToLower() == "player" && b.transform.parent.name.ToLower().Contains("char"))
            {
                dialGO = b;
            }
        }

        private void OnBlockEnd(Block block)
        {
            if (block != null)
            {
                dialGO = playerGO;
            }

            //Debug.Log("Block End.");
        }
    }
}