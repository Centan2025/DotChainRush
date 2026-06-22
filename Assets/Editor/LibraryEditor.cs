#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(DotChainRushLibrary))]
public class LibraryEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DotChainRushLibrary kütüphane = (DotChainRushLibrary)target;

        GUILayout.Space(15);
        
        if (GUILayout.Button("TÜM TOPLARI OTOMATİK EŞLEŞTİR", GUILayout.Height(35)))
        {
            if (kütüphane.anaGorsel == null)
            {
                EditorUtility.DisplayDialog("Hata", "Lütfen önce 'Ana Gorsel' kısmına dilimlediğiniz Sprite Sheet'i atın!", "Tamam");
                return;
            }

            // Dosya yolunu alıyoruz
            string dosyaYolu = AssetDatabase.GetAssetPath(kütüphane.anaGorsel);
            
            // Dosyanın altındaki tüm dilimlenmiş Sprite'ları yüklüyoruz
            object[] altOgeler = AssetDatabase.LoadAllAssetsAtPath(dosyaYolu);
            
            // Enum listesindeki toplam eleman sayısı
            int enumUzunlugu = Enum.GetValues(typeof(TopTipi)).Length;
            kütüphane.tumToplar = new TopGorselEslestirme[enumUzunlugu];

            int bulunanSpriteIndex = 0;

            // Alt ögeler arasında dönüp sadece Sprite olanları ayıklıyoruz
            foreach (object oge in altOgeler)
            {
                if (oge is Sprite sprite)
                {
                    // Görseldeki sıralamaya göre enum eşleştirmesi yapılıyor
                    if (bulunanSpriteIndex < enumUzunlugu)
                    {
                        kütüphane.tumToplar[bulunanSpriteIndex] = new TopGorselEslestirme
                        {
                            topTipi = (TopTipi)bulunanSpriteIndex,
                            topSprite = sprite
                        };
                        bulunanSpriteIndex++;
                    }
                }
            }

            // Değişiklikleri Unity editörüne kaydetmesi için zorluyoruz
            EditorUtility.SetDirty(kütüphane);
            AssetDatabase.SaveAssets();
            
            EditorUtility.DisplayDialog("Başarılı", $"{bulunanSpriteIndex} adet top görseldeki sırayla otomatik olarak eşleştirildi!", "Harika");
        }
    }
}
#endif
