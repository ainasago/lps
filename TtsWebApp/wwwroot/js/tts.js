// TTS 页面的 JavaScript 逻辑
let voices = [];
let currentAudioData = null;
let currentSubtitles = null;

$(document).ready(function() {
    loadVoices();
    
    $('#textInput').on('input', function() {
        $('#charCount').text($(this).val().length + ' / 5000');
    });
    
    $('#pitchRange').on('input', function() {
        $('#pitchValue').text($(this).val());
    });
    
    $('#rateRange').on('input', function() {
        $('#rateValue').text($(this).val());
    });
    
    $('#volumeRange').on('input', function() {
        $('#volumeValue').text($(this).val());
    });
    
    $('#languageSelect').on('change', function() {
        updateVoiceList($(this).val());
    });
    
    $('#convertBtn').on('click', function() {
        convertText();
    });
    
    $('#playBtn').on('click', function() {
        playAudio();
    });
    
    $('#downloadAudioBtn').on('click', function() {
        downloadAudio();
    });
    
    $('#downloadSubtitlesBtn').on('click', function() {
        downloadSubtitles();
    });
    
    const audioPlayer = document.getElementById('audioPlayer');
    audioPlayer.addEventListener('timeupdate', updateProgress);
    audioPlayer.addEventListener('ended', function() {
        $('#playBtn .material-symbols-outlined').text('play_arrow');
    });
});

function loadVoices() {
    $.get('/Tts/GetVoices', function(data) {
        voices = data;
        populateLanguageSelect();
    }).fail(function() {
        alert('加载语音列表失败');
    });
}

function populateLanguageSelect() {
    const languages = [...new Set(voices.map(v => v.locale))];
    const languageSelect = $('#languageSelect');
    languageSelect.empty().append('<option value="">选择语言...</option>');
    languages.sort();
    const localeNameMap = {};
    voices.forEach(voice => {
        if (!localeNameMap[voice.locale] && voice.localeName) {
            localeNameMap[voice.locale] = voice.localeName;
        }
    });
    languages.forEach(lang => {
        const displayName = localeNameMap[lang] || lang;
        languageSelect.append(`<option value="${lang}">${displayName}</option>`);
    });
    
    // 自动检测浏览器语言并选择默认语言
    autoSelectLanguage();
}

function autoSelectLanguage() {
    // 获取浏览器语言
    const browserLang = navigator.language || navigator.userLanguage;
    
    // 语言映射表
    const langMap = {
        'zh': 'zh-CN',
        'zh-CN': 'zh-CN',
        'zh-TW': 'zh-TW',
        'zh-HK': 'zh-HK',
        'en': 'en-US',
        'en-US': 'en-US',
        'en-GB': 'en-GB',
        'ja': 'ja-JP',
        'ko': 'ko-KR',
        'fr': 'fr-FR',
        'de': 'de-DE',
        'es': 'es-ES'
    };
    
    // 尝试精确匹配
    let matchedLocale = null;
    const browserLangLower = browserLang.toLowerCase();
    
    // 首先尝试完全匹配
    for (const [key, value] of Object.entries(langMap)) {
        if (browserLangLower === key.toLowerCase()) {
            matchedLocale = value;
            break;
        }
    }
    
    // 如果没有完全匹配，尝试匹配语言代码（如 zh-CN 匹配 zh）
    if (!matchedLocale) {
        const langCode = browserLangLower.split('-')[0];
        matchedLocale = langMap[langCode];
    }
    
    // 获取可用语言列表
    const availableLocales = voices.map(v => v.locale);
    let selectedLocale = null;
    
    // 检查匹配的语言是否在可用语言列表中
    if (matchedLocale) {
        const exactMatch = availableLocales.find(l => l === matchedLocale);
        
        if (exactMatch) {
            selectedLocale = exactMatch;
        } else {
            // 尝试模糊匹配（如 zh-CN 匹配 zh-*）
            const langPrefix = matchedLocale.split('-')[0];
            const fuzzyMatch = availableLocales.find(l => l.startsWith(langPrefix));
            if (fuzzyMatch) {
                selectedLocale = fuzzyMatch;
            }
        }
    }
    
    // 如果没有匹配到任何语言，默认选择英文
    if (!selectedLocale) {
        // 尝试找 en-US
        selectedLocale = availableLocales.find(l => l === 'en-US');
        
        // 如果没有 en-US，找任何 en- 开头的
        if (!selectedLocale) {
            selectedLocale = availableLocales.find(l => l.startsWith('en-'));
        }
        
        // 如果还是没有，选择第一个可用语言
        if (!selectedLocale && availableLocales.length > 0) {
            selectedLocale = availableLocales[0];
        }
    }
    
    // 设置选中的语言
    if (selectedLocale) {
        $('#languageSelect').val(selectedLocale);
        updateVoiceList(selectedLocale);
        
        // 自动选择第一个配音员
        setTimeout(() => {
            const firstVoice = $('#voiceSelect option:eq(1)').val();
            if (firstVoice) {
                $('#voiceSelect').val(firstVoice);
            }
        }, 100);
    }
}

function updateVoiceList(selectedLanguage) {
    const voiceSelect = $('#voiceSelect');
    voiceSelect.empty().append('<option value="">选择配音员...</option>');
    if (selectedLanguage) {
        const filteredVoices = voices.filter(v => v.locale === selectedLanguage);
        filteredVoices.forEach(voice => {
            const displayName = voice.shortName || voice.name;
            voiceSelect.append(`<option value="${voice.shortName}">${displayName} (${voice.gender})</option>`);
        });
    }
}

function convertText() {
    const text = $('#textInput').val().trim();
    const voiceId = $('#voiceSelect').val();
    const language = $('#languageSelect').val();
    const pitch = parseInt($('#pitchRange').val());
    const rate = parseInt($('#rateRange').val());
    const volume = parseInt($('#volumeRange').val());
    const generateSubtitles = $('#generateSubtitles').is(':checked');
    
    // 高级功能参数
    const previewMode = $('#previewMode').is(':checked');
    const previewSentences = parseInt($('#previewSentences').val()) || 3;
    const breakTime = parseInt($('#breakTime').val()) || 0;
    const enableLongTextSplit = $('#enableLongTextSplit').is(':checked');
    const maxCharsPerChunk = parseInt($('#maxCharsPerChunk').val()) || 500;
    
    if (!text) {
        alert('请输入要转换的文本');
        return;
    }
    if (!voiceId) {
        alert('请选择配音员');
        return;
    }
    
    const request = {
        Text: text,
        Voice: voiceId,
        Language: language,
        Pitch: pitch,
        Rate: rate,
        Volume: volume,
        GenerateSubtitles: generateSubtitles,
        // 高级功能
        PreviewMode: previewMode,
        PreviewSentences: previewSentences,
        BreakTime: breakTime,
        EnableLongTextSplit: enableLongTextSplit,
        MaxCharsPerChunk: maxCharsPerChunk
    };
    
    const $convertBtn = $('#convertBtn');
    $convertBtn.prop('disabled', true).find('span.truncate').text('转换中...');
    
    $.ajax({
        url: '/Tts/ConvertText',
        type: 'POST',
        contentType: 'application/json',
        data: JSON.stringify(request),
        success: function(response) {
            if (response.success) {
                currentAudioData = response.audioData;
                currentSubtitles = response.subtitles;
                
                // 显示元数据信息
                if (response.chunkCount || response.processingTimeMs || response.isPreview) {
                    let metaInfo = [];
                    if (response.isPreview) metaInfo.push('🎧 试听模式');
                    if (response.chunkCount > 1) metaInfo.push(`📄 ${response.chunkCount} 个片段`);
                    if (response.totalCharacters) metaInfo.push(`📝 ${response.totalCharacters} 字符`);
                    if (response.processingTimeMs) metaInfo.push(`⏱️ ${(response.processingTimeMs / 1000).toFixed(1)}秒`);
                    
                    // 可以在页面上显示这些信息
                }
                
                $('#inlinePlayer').show();
                
                const audioPlayer = document.getElementById('audioPlayer');
                audioPlayer.src = 'data:audio/mpeg;base64,' + currentAudioData;
                audioPlayer.onerror = function(e) {
                    alert('音频加载失败');
                };
                audioPlayer.onloadedmetadata = function() {
                    updateTimeDisplay();
                };
                audioPlayer.load();
                
                if (currentSubtitles) {
                    $('#subtitlesContainer').show();
                    $('#subtitlesContent').text(currentSubtitles);
                } else {
                    $('#subtitlesContainer').hide();
                }
            } else {
                alert('转换失败: ' + response.errorMessage);
            }
        },
        error: function(xhr, status, error) {
            alert('转换请求失败');
        },
        complete: function() {
            $convertBtn.prop('disabled', false).find('span.truncate').text('生成语音');
        }
    });
}

function playAudio() {
    const audioPlayer = document.getElementById('audioPlayer');
    const $playBtn = $('#playBtn .material-symbols-outlined');
    if (audioPlayer.paused) {
        audioPlayer.play();
        $playBtn.text('pause');
    } else {
        audioPlayer.pause();
        $playBtn.text('play_arrow');
    }
}

function updateProgress() {
    const audioPlayer = document.getElementById('audioPlayer');
    if (audioPlayer.duration) {
        const progress = (audioPlayer.currentTime / audioPlayer.duration) * 100;
        $('#progressBar').css('width', progress + '%');
        updateTimeDisplay();
    }
}

function updateTimeDisplay() {
    const audioPlayer = document.getElementById('audioPlayer');
    const current = formatTime(audioPlayer.currentTime);
    const duration = formatTime(audioPlayer.duration);
    $('#timeDisplay').text(current + ' / ' + duration);
}

function formatTime(seconds) {
    if (isNaN(seconds)) return '0:00';
    const mins = Math.floor(seconds / 60);
    const secs = Math.floor(seconds % 60);
    return mins + ':' + (secs < 10 ? '0' : '') + secs;
}

function downloadAudio() {
    if (currentAudioData) {
        const a = document.createElement('a');
        a.href = 'data:audio/mpeg;base64,' + currentAudioData;
        a.download = 'tts_audio_' + new Date().getTime() + '.mp3';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    }
}

function downloadSubtitles() {
    if (currentSubtitles) {
        const a = document.createElement('a');
        a.href = 'data:text/plain;charset=utf-8,' + encodeURIComponent(currentSubtitles);
        a.download = 'tts_subtitles_' + new Date().getTime() + '.srt';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    }
}
