/*
Function: 		Provide generic map to load and store game content AND allow dispose() to be called on all content
Author: 		NMCG
Version:		1.0
Date Updated:	11/9/17
Bugs:			None
Fixes:			None
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;

namespace GDLibrary
{
    public class ContentDictionary<V> : IDisposable
    {
        public ContentDictionary(string name, ContentManager content)
        {
            Name = name;
            this.content = content;
            Dictionary = new Dictionary<string, V>();
        }

        public virtual void Dispose()
        {
            //copy values from dictionary to list
            var list = new List<V>(Dictionary.Values);

            for (var i = 0; i < list.Count; i++)
                Dispose(list[i]);

            //empty the list
            list.Clear();

            //clear the dictionary
            Dictionary.Clear();
        }

        public virtual bool Load(string assetPath, string key)
        {
            if (!Dictionary.ContainsKey(key))
            {
                Dictionary.Add(key, content.Load<V>(assetPath));
                return true;
            }

            return false;
        }

        //same as Load() above but uses assetPath to form key string from regex
        public virtual bool Load(string assetPath)
        {
            return Load(assetPath, StringUtility.ParseNameFromPath(assetPath));
        }

        public virtual bool Unload(string key)
        {
            if (Dictionary.ContainsKey(key))
            {
                //unload from RAM
                Dispose(Dictionary[key]);
                //remove from dictionary
                Dictionary.Remove(key);
                return true;
            }

            return false;
        }

        public virtual int Count()
        {
            return Dictionary.Count;
        }

        public virtual void Dispose(V value)
        {
            //if this is a disposable object (e.g. model, sound, font, texture) then call its dispose
            if (value is IDisposable)
                ((IDisposable) value).Dispose();
            //if it's just a user-defined or C# object, then set to null for garbage collection
            else
                value = default(V); //null
        }

        #region Fields

        private readonly ContentManager content;

        #endregion

        #region Properties

        protected Dictionary<string, V> Dictionary { get; }

        public V this[string key]
        {
            get
            {
                if (!Dictionary.ContainsKey(key))
                    throw new Exception(key + " resource was not found in dictionary. Have you loaded it?");

                return Dictionary[key];
            }
        }

        public string Name { get; set; }

        #endregion
    }
}