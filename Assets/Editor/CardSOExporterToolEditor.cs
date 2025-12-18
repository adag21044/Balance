using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text;
using System.Linq;

[CustomEditor(typeof(CardSOExporterTool))]
public class CardSOExporterToolEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Export CardSO to CSV"))
        {
            Export();
        }
    }

    private void Export()
    {
        string[] guids = AssetDatabase.FindAssets("t:CardSO");
        StringBuilder csv = new StringBuilder();

        csv.AppendLine(
            "Id,Title,Description,Artwork," +
            "LeftAnswer,RightAnswer," +
            "LeftHeart,LeftCareer,LeftHappiness,LeftSociability," +
            "RightHeart,RightCareer,RightHappiness,RightSociability," +
            "ImpactTypes,AgeImpact," +
            "IsChainCard,NextOnLeft,NextOnRight," +
            "NextPoolLeft,NextPoolRight," +
            "LifeStage,IsOnlyOnce"
        );

        foreach (string guid in guids)
        {
            CardSO card = AssetDatabase.LoadAssetAtPath<CardSO>(
                AssetDatabase.GUIDToAssetPath(guid)
            );

            csv.AppendLine(string.Join(",",
                Escape(card.Id),
                Escape(card.Title),
                Escape(card.Description),
                Escape(card.Artwork ? card.Artwork.name : ""),
                Escape(card.leftAnswer),
                Escape(card.rightAnswer),

                card.leftHeartImpact,
                card.leftCareerImpact,
                card.leftHappinessImpact,
                card.leftSociabilityImpact,

                card.rightHeartImpact,
                card.rightCareerImpact,
                card.rightHappinessImpact,
                card.rightSociabilityImpact,

                Escape(string.Join("|", card.impactType.Select(i => i.ToString()))),
                card.ageImpact,

                card.isChainCard,
                Escape(card.nextOnLeft ? card.nextOnLeft.Id : ""),
                Escape(card.nextOnRight ? card.nextOnRight.Id : ""),
                Escape(JoinIds(card.nextPoolLeft)),
                Escape(JoinIds(card.nextPoolRight)),

                card.lifeStage,
                card.isOnlyOnce
            ));
        }

        string path = Application.dataPath + "/CardSO_Export.csv";
        File.WriteAllText(path, csv.ToString(), Encoding.UTF8);
        AssetDatabase.Refresh();

        Debug.Log("CSV Exported → " + path);
    }

    private string JoinIds(CardSO[] cards)
    {
        if (cards == null || cards.Length == 0)
            return "";
        return string.Join("|", cards.Where(c => c != null).Select(c => c.Id));
    }

    private string Escape(string v)
    {
        if (string.IsNullOrEmpty(v)) return "";
        return $"\"{v.Replace("\"", "\"\"")}\"";
    }
}
