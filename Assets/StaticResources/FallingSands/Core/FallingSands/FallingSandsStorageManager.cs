using System;
using System.IO;
using System.Linq;
using System.Text;
using Unity.Collections;
using UnityEditor;

namespace CorgiFallingSands
{
    using System.Collections;
    using System.Collections.Generic;
    using Unity.Jobs;
    using Unity.Mathematics;
    using UnityEngine;

    /// <summary>
    /// Used for storing and loading falling sands chunks from the filesystem. 
    /// ScheduleSaveChunk and ScheduleLoadChunk are marked as virtual, so you can override them if needed for non-PC platforms.
    /// </summary>
    public class FallingSandsStorageManager : MonoBehaviour
    {
        private const string LevelDirectoryName = "Levels";
        private const string LevelsRegistryFileName = "LevelsRegistry";

        public int ChunkToSave;
        public int ChunkToLoad;
        public string SaveDataSubFolder = "chunks";


        public virtual string GetChunkSaveFolder()
        {
            //return $"{Application.persistentDataPath}/{SaveDataSubFolder}";
            var levelsDirectoryPath = $"{Application.dataPath}/Resources/{LevelDirectoryName}";

            if (!Directory.Exists(levelsDirectoryPath))
            {
                Directory.CreateDirectory(levelsDirectoryPath);
            }

            return levelsDirectoryPath;
        }

        public virtual string GetChunkSaveFile(int level)
        {
            return $"{GetChunkSaveFolder()}/{level}_level.txt";
        }

        public virtual string GetChunkLoadFile(int level)
        {
            return $"{LevelDirectoryName}/{level}_level";
        }

        /// <summary>
        /// Schedules a chunk to be saved to disk. 
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="dependency"></param>
        /// <returns></returns>
        public virtual JobHandle ScheduleSaveChunk(FallingSandsChunk chunk, JobHandle dependency)
        {
            RegisterLevel(ChunkToSave);

            var filename = GetChunkSaveFile(ChunkToSave);

            var job = new SaveChunkJob()
            {
                SaveData = chunk.DataTex,
                SaveTemp = chunk.TempTex,
                filepathHandle = System.Runtime.InteropServices.GCHandle.Alloc(filename),
            };

            return job.Schedule(dependency);
        }

        public virtual void RegisterLevel(int level)
        {
            var levelsRegistryPath = $"{GetChunkSaveFolder()}/{LevelsRegistryFileName}.txt";

            if (!File.Exists(levelsRegistryPath))
                File.Create(levelsRegistryPath);
            

            var levels = GatherStoredChunks();
            if (levels.Select(l => l.x).Contains(level))
            {
                return;
            }
            
            var fileText = File.ReadAllText(levelsRegistryPath);

            if (!fileText.EndsWith(','))
            {
                fileText += ",";
            }

            fileText += level.ToString();

            File.WriteAllText(levelsRegistryPath, fileText, Encoding.UTF8);
        }

        /// <summary>
        /// Schedules a chunk to be loaded from disk. 
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="dependency"></param>
        /// <returns></returns>
        public virtual JobHandle ScheduleLoadChunk(FallingSandsChunk chunk, JobHandle dependency)
        {
            var filename = GetChunkLoadFile(ChunkToLoad);

            var level = Resources.Load<TextAsset>(filename);

            if (level == null)
            {
                return dependency;
            }

            var job = new LoadChunkJob()
            {
                SaveData = chunk.DataTex,
                SaveTemp = chunk.TempTex,
                StorageArray = new NativeArray<byte>(level.bytes, Allocator.TempJob)
            };

            return job.Schedule(dependency);
        }

        /// <summary>
        /// Queries the filesystem for a list of stored chunks. Blocks the main thread. 
        /// </summary>
        /// <returns></returns>
        public virtual List<int2> GatherStoredChunks()
        {
            var list = new List<int2>();

            var levelsRegistry = Resources.Load<TextAsset>($"{LevelDirectoryName}/{LevelsRegistryFileName}");

            if (levelsRegistry == null)
            {
                return new List<int2>();
            }


            /* var chunkDirectory = GetChunkSaveFolder();
             if (!System.IO.Directory.Exists(chunkDirectory))
             {
                 System.IO.Directory.CreateDirectory(chunkDirectory);
                 return list;
             }*/

            //var files = System.IO.Directory.GetFiles(chunkDirectory);
            foreach (var levelIndex in levelsRegistry.text.Split(',',StringSplitOptions.RemoveEmptyEntries))
            {
                if (int.TryParse(levelIndex, out int index))
                    list.Add(new int2(index, 0));

                /* var filename = System.IO.Path.GetFileNameWithoutExtension(file);
                 var positions = filename.Split('_');
 
                 if (positions.Length < 2)
                 {
                     continue;
                 }
 
                 var parsed_x = int.TryParse(positions[0], out int x_pos);
                 var parsed_y = int.TryParse(positions[1], out int y_pos);
 
                 if (!parsed_x || !parsed_y)
                 {
                     continue;
                 }
 
                 list.Add(new int2(x_pos, y_pos));*/
            }

            return list;
        }
    }
}