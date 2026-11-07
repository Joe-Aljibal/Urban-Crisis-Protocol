using UnityEditor;
using UnityEngine;

// Les morceaux de Resources/Music sont lus en streaming : ils ne sont pas charges en entier
// en memoire (une musique de 3 min decompressee pese environ 30 Mo).
public class MusicImportSettings : AssetPostprocessor
{
    void OnPreprocessAudio()
    {
        if (!assetPath.Contains("/Resources/Music/"))
            return;

        var importer = (AudioImporter)assetImporter;
        AudioImporterSampleSettings settings = importer.defaultSampleSettings;
        settings.loadType = AudioClipLoadType.Streaming;
        settings.compressionFormat = AudioCompressionFormat.Vorbis;
        settings.quality = 0.7f;
        importer.defaultSampleSettings = settings;
    }
}
