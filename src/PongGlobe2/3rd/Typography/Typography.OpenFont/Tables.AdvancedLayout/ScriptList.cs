﻿//Apache2, 2016-2017, WinterDev

using System.IO;

namespace Typography.OpenFont.Tables
{

    public class ScriptList
    {
        //https://www.microsoft.com/typography/otspec/chapter2.htm
        //The ScriptList identifies the scripts in a font, 
        //each of which is represented by a Script table that contains script and language-system data.
        //Language system tables reference features, which are defined in the FeatureList. 
        //Each feature table references the lookup data defined in the LookupList that describes how, when, and where to implement the feature.


        ScriptTable[] scriptTables;
        struct ScriptRecord
        {
            public readonly uint scriptTag;//4-byte ScriptTag identifier
            public readonly ushort offset; //Script Offset to Script table-from beginning of ScriptList
            public ScriptRecord(uint scriptTag, ushort offset)
            {
                this.scriptTag = scriptTag;
                this.offset = offset;
            }
            public string ScriptName
            {
                get { return Utils.TagToString(scriptTag); }
            }
#if DEBUG
            public override string ToString()
            {
                return ScriptName + "," + offset;
            }
#endif
        }


        public ScriptTable FindScriptTable(string scriptTagName)
        {
            for (int i = scriptTables.Length - 1; i >= 0; --i)
            {
                if (scriptTables[i].ScriptTagName == scriptTagName)
                {
                    return scriptTables[i];
                }
            }
            return null;
        }

        public static ScriptList CreateFrom(BinaryReader reader, long beginAt)
        {



            reader.BaseStream.Seek(beginAt, SeekOrigin.Begin);

            //https://www.microsoft.com/typography/otspec/chapter2.htm
            //ScriptList table
            //Type 	    Name 	                    Description
            //uint16 	ScriptCount             	Number of ScriptRecords
            //struct 	ScriptRecord[ScriptCount] 	Array of ScriptRecords
            //                                      -listed alphabetically by ScriptTag
            //ScriptRecord
            //Type 	    Name 	       Description
            //Tag 	    ScriptTag 	   4-byte ScriptTag identifier
            //Offset16 	Script 	       Offset to Script table-from beginning of ScriptList

            ScriptList scriptList = new ScriptList();
            ushort scriptCount = reader.ReadUInt16();
            ScriptRecord[] scRecords = new ScriptRecord[scriptCount];
            for (int i = 0; i < scriptCount; ++i)
            {
                //read script record
                scRecords[i] = new ScriptRecord(
                    reader.ReadUInt32(),//tag (4-byte ScriptTag identifier, so I read as UInt32
                    reader.ReadUInt16());//offset                 
            }
            //-------------
            ScriptTable[] scriptTables = scriptList.scriptTables = new ScriptTable[scriptCount];
            //then read each
            for (int i = 0; i < scriptCount; ++i)
            {
                ScriptRecord scriptRecord = scRecords[i];
                //move to
                ScriptTable scriptTable = ScriptTable.CreateFrom(reader, beginAt + scriptRecord.offset);
                scriptTable.scriptTag = scriptRecord.scriptTag;
                scriptTables[i] = scriptTable;
            }
            return scriptList;
        }


    }




}