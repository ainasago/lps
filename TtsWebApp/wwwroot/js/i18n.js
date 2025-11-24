// 多语言支持
const translations = {
    'zh-CN': {
        // 导航
        'nav.home': '首页',
        'nav.about': '关于我们',
        'nav.contact': '联系我们',
        'nav.features': '功能特性',
        
        // 首页
        'home.title': '安妮语音转换 - 免费AI文字转语音工具',
        'home.subtitle': '支持多种语言和自然人声，为您的视频、播客和演示文稿创建高质量音频',
        'home.neuralNetworkTts': '神经网络语音合成',
        'home.inputLabel': '在此输入您的文本',
        'home.charCount': '{0} / 5000',
        'home.placeholder': '粘贴或输入您的文本...',
        'home.language': '语言',
        'home.selectLanguage': '选择语言...',
        'home.voice': '配音员',
        'home.selectVoice': '选择配音员...',
        'home.pitch': '音调',
        'home.rate': '语速',
        'home.volume': '音量',
        'home.generateSubtitles': '生成字幕',
        'home.generateSpeech': '生成语音',
        'home.play': '播放',
        'home.download': '下载音频',
        'home.downloadSubtitles': '下载字幕',
        'home.converting': '转换中...',
        'home.subtitlesTitle': '字幕内容',
        
        // Features Section
        'features.title': '为什么选择我们',
        'features.subtitle': '探索让我们的 TTS 服务脱颖而出的功能特性',
        'features.globalLanguage': '全球语言覆盖',
        'features.globalLanguageDesc': '支持全球数十种语言和方言',
        'features.aiVoice': 'AI 驱动的自然语音',
        'features.aiVoiceDesc': '利用尖端 AI 技术实现极其逼真的人声',
        'features.multiFormat': '多种音频格式',
        'features.multiFormatDesc': '导出 MP3、WAV 等流行格式的音频',
        'features.devApi': '开发者 API',
        'features.devApiDesc': '轻松将我们强大的 TTS 引擎集成到您的应用中',
        
        // Use Cases
        'usecases.title': '适用于任何场景',
        'usecases.subtitle': '从创意项目到商业应用，我们都能满足您的需求',
        'usecases.video': '视频和播客旁白',
        'usecases.videoDesc': '无需麦克风即可为您的内容创建专业配音',
        'usecases.education': '在线学习与教育',
        'usecases.educationDesc': '为课程和教育内容制作引人入胜的音频材料',
        'usecases.marketing': '营销与广告',
        'usecases.marketingDesc': '在几分钟内制作引人注目的音频广告和宣传内容',
        
        // Footer
        'footer.brandDesc': '免费在线AI文字转语音工具<br/>支持多种语言和自然人声',
        'footer.products': '产品服务',
        'footer.onlineTts': '在线配音',
        'footer.articleList': '文章列表',
        'footer.about': '关于',
        'footer.aboutUs': '关于我们',
        'footer.contactUs': '联系我们',
        'footer.privacy': '隐私政策',
        'footer.terms': '服务条款',
        'footer.contact': '联系方式',
        'footer.email': '邮箱',
        'footer.onlineMessage': '在线留言',
        'footer.disclaimer': '免责声明',
        
        // Messages
        'msg.enterText': '请输入要转换的文本',
        'msg.selectVoice': '请选择配音员',
        'msg.convertFailed': '转换失败',
        'msg.audioLoadFailed': '音频加载失败，请检查控制台日志',
        'msg.audioLoaded': '音频元数据加载完成，时长',
        'msg.loadVoicesFailed': '加载语音列表失败',
        'msg.converting': '转换中...',
        'msg.generateSpeech': '生成语音',
        'msg.audioLoadError': '音频加载失败',
        'msg.convertError': '转换失败: ',
        'msg.convertRequestFailed': '转换请求失败',
        
        // Article
        'article.backToList': '返回文章列表',
        'article.viewChinese': '查看中文版',
        'article.viewEnglish': 'View English Version',
        
        // TTS Page
        'tts.selectLanguage': '选择语言...',
        'tts.selectVoice': '选择配音员...',
        'tts.advancedOptions': '高级选项',
        'tts.previewMode': '试听模式（前',
        'tts.sentences': '句）',
        'tts.sentencePause': '句间停顿：',
        'tts.milliseconds': '毫秒',
        'tts.noPause': '(0=无停顿)',
        'tts.longTextSlice': '长文本切片（每片',
        'tts.characters': '字）',
        'tts.subtitlesContent': '字幕内容',
        'tts.downloadAudio': '下载音频',
        'tts.downloadSubtitles': '下载字幕'
    },
    'en-US': {
        // Navigation
        'nav.home': 'Home',
        'nav.about': 'About Us',
        'nav.contact': 'Contact',
        'nav.features': 'Features',
        
        // Home
        'home.title': 'Annie TTS - Free AI Text to Speech Tool',
        'home.subtitle': 'Create high-quality audio for your videos, podcasts, and presentations with multiple languages and natural voices',
        'home.neuralNetworkTts': 'Neural Network Speech Synthesis',
        'home.inputLabel': 'Enter your text here',
        'home.charCount': '{0} / 5000',
        'home.placeholder': 'Paste or type your text here...',
        'home.language': 'Language',
        'home.selectLanguage': 'Select language...',
        'home.voice': 'Voice',
        'home.selectVoice': 'Select voice...',
        'home.pitch': 'Pitch',
        'home.rate': 'Speed',
        'home.volume': 'Volume',
        'home.generateSubtitles': 'Generate Subtitles',
        'home.generateSpeech': 'Generate Speech',
        'home.play': 'Play',
        'home.download': 'Download Audio',
        'home.downloadSubtitles': 'Download Subtitles',
        'home.converting': 'Converting...',
        'home.subtitlesTitle': 'Subtitles',
        
        // Features Section
        'features.title': 'Why Choose Us',
        'features.subtitle': 'Discover the features that make our TTS service stand out',
        'features.globalLanguage': 'Global Language Coverage',
        'features.globalLanguageDesc': 'Support for dozens of languages and dialects worldwide',
        'features.aiVoice': 'AI-Powered Natural Voices',
        'features.aiVoiceDesc': 'Leverage cutting-edge AI for incredibly human-like speech',
        'features.multiFormat': 'Multiple Audio Formats',
        'features.multiFormatDesc': 'Export your audio in MP3, WAV, and other popular formats',
        'features.devApi': 'Developer API',
        'features.devApiDesc': 'Easily integrate our powerful TTS engine into your applications',
        
        // Use Cases
        'usecases.title': 'Perfect for Any Scenario',
        'usecases.subtitle': 'From creative projects to business applications, we\'ve got you covered',
        'usecases.video': 'Video & Podcast Narration',
        'usecases.videoDesc': 'Create professional voice-overs for your content without a microphone',
        'usecases.education': 'E-Learning & Education',
        'usecases.educationDesc': 'Produce engaging audio materials for courses and educational content',
        'usecases.marketing': 'Marketing & Advertising',
        'usecases.marketingDesc': 'Craft compelling audio ads and promotional content in minutes',
        
        // Footer
        'footer.brandDesc': 'Free online AI text-to-speech tool<br/>Multiple languages and natural voices',
        'footer.products': 'Products',
        'footer.onlineTts': 'Online TTS',
        'footer.articleList': 'Article List',
        'footer.about': 'About',
        'footer.aboutUs': 'About Us',
        'footer.contactUs': 'Contact Us',
        'footer.privacy': 'Privacy Policy',
        'footer.terms': 'Terms of Service',
        'footer.contact': 'Contact',
        'footer.email': 'Email',
        'footer.onlineMessage': 'Leave a Message',
        'footer.disclaimer': 'Disclaimer',
        
        // Messages
        'msg.enterText': 'Please enter text to convert',
        'msg.selectVoice': 'Please select a voice',
        'msg.convertFailed': 'Conversion failed',
        'msg.audioLoadFailed': 'Audio loading failed, please check console',
        'msg.audioLoaded': 'Audio metadata loaded, duration',
        'msg.loadVoicesFailed': 'Failed to load voice list',
        'msg.converting': 'Converting...',
        'msg.generateSpeech': 'Generate Speech',
        'msg.audioLoadError': 'Audio loading failed',
        'msg.convertError': 'Conversion failed: ',
        'msg.convertRequestFailed': 'Conversion request failed',
        
        // Article
        'article.backToList': 'Back to Article List',
        'article.viewChinese': '查看中文版',
        'article.viewEnglish': 'View English Version',
        
        // TTS Page
        'tts.selectLanguage': 'Select language...',
        'tts.selectVoice': 'Select voice...',
        'tts.advancedOptions': 'Advanced Options',
        'tts.previewMode': 'Preview mode (first',
        'tts.sentences': ' sentences)',
        'tts.sentencePause': 'Sentence pause: ',
        'tts.milliseconds': 'ms',
        'tts.noPause': '(0=no pause)',
        'tts.longTextSlice': 'Long text slicing (per',
        'tts.characters': ' characters)',
        'tts.subtitlesContent': 'Subtitles',
        'tts.downloadAudio': 'Download Audio',
        'tts.downloadSubtitles': 'Download Subtitles'
    }
};

// 当前语言
let currentLang = localStorage.getItem('siteLang') || 'zh-CN';

// 获取翻译文本
function t(key, ...args) {
    let text = translations[currentLang][key] || translations['zh-CN'][key] || key;
    
    // 替换占位符 {0}, {1}, etc.
    args.forEach((arg, index) => {
        text = text.replace(`{${index}}`, arg);
    });
    
    return text;
}

// 切换语言
function switchLanguage(lang) {
    currentLang = lang;
    localStorage.setItem('siteLang', lang);
    
    // 检查是否在静态页面上，如果是则跳转到对应语言的页面
    const currentPath = window.location.pathname;
    const staticPages = ['about', 'privacy', 'terms', 'disclaimer', 'contact'];
    
    // 检查当前路径是否是静态页面
    for (const page of staticPages) {
        // 匹配 /page 或 /page-en 格式
        if (currentPath === `/${page}` || currentPath === `/${page}-en`) {
            // 根据目标语言构建新路径
            const newPath = lang === 'en-US' ? `/${page}-en` : `/${page}`;
            
            // 如果路径不同，则跳转
            if (currentPath !== newPath) {
                window.location.href = newPath;
                return; // 跳转后不再执行后续代码
            }
            break;
        }
    }
    
    // 如果不需要跳转，则更新页面语言
    updatePageLanguage();
    
    // 更新 HTML lang 属性
    document.documentElement.lang = lang === 'zh-CN' ? 'zh-CN' : 'en';
}

// 更新页面语言
function updatePageLanguage() {
    // 更新所有带 data-i18n 属性的元素
    document.querySelectorAll('[data-i18n]').forEach(el => {
        const key = el.getAttribute('data-i18n');
        
        // 检查元素是否有 data-zh 和 data-en 属性
        const zhText = el.getAttribute('data-zh');
        const enText = el.getAttribute('data-en');
        
        if (zhText && enText) {
            // 如果有 data-zh 和 data-en 属性，使用它们
            el.textContent = currentLang === 'zh-CN' ? zhText : enText;
        } else {
            // 否则使用翻译表
            const text = t(key);
            
            if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') {
                if (el.hasAttribute('placeholder')) {
                    el.placeholder = text;
                } else {
                    el.value = text;
                }
            } else {
                el.innerHTML = text;
            }
        }
    });
    
    // 更新网站标题（使用 data-zh 和 data-en 属性）
    const siteNameEl = document.getElementById('siteName');
    if (siteNameEl) {
        const zhName = siteNameEl.getAttribute('data-zh');
        const enName = siteNameEl.getAttribute('data-en');
        siteNameEl.textContent = currentLang === 'zh-CN' ? zhName : enName;
    }
    
    // 更新页面链接（中英文页面切换）
    updatePageLinks();
    
    // 更新页面标题
    const titleKey = document.querySelector('[data-i18n-title]')?.getAttribute('data-i18n-title');
    if (titleKey) {
        const siteName = currentLang === 'zh-CN' ? 
            (siteNameEl?.getAttribute('data-zh') || '安妮语音转换') : 
            (siteNameEl?.getAttribute('data-en') || 'Annie TTS');
        document.title = t(titleKey) + ' - ' + siteName;
    }
}

// 更新页面链接（根据语言切换中英文页面）
function updatePageLinks() {
    const pageLinks = ['about', 'privacy', 'terms', 'disclaimer', 'contact'];
    
    pageLinks.forEach(page => {
        // 查找所有指向该页面的链接
        const links = document.querySelectorAll(`a[href="/${page}"]`);
        links.forEach(link => {
            if (currentLang === 'en-US') {
                link.href = `/${page}-en`;
            } else {
                link.href = `/${page}`;
            }
        });
    });
}

// 初始化语言
function initLanguage() {
    // 检查当前页面路径，判断是否是英文页面
    const currentPath = window.location.pathname;
    const isEnglishPage = currentPath.endsWith('-en');
    
    // 如果是英文页面，自动设置为英文
    if (isEnglishPage) {
        currentLang = 'en-US';
        localStorage.setItem('siteLang', 'en-US');
    } 
    // 如果是中文页面（about, privacy, terms, disclaimer, contact），自动设置为中文
    else if (/^\/(about|privacy|terms|disclaimer|contact)$/.test(currentPath)) {
        currentLang = 'zh-CN';
        localStorage.setItem('siteLang', 'zh-CN');
    }
    // 其他页面使用保存的语言设置或浏览器语言
    else if (!localStorage.getItem('siteLang')) {
        const browserLang = navigator.language || navigator.userLanguage;
        currentLang = browserLang.startsWith('zh') ? 'zh-CN' : 'en-US';
        localStorage.setItem('siteLang', currentLang);
    }
    
    updatePageLanguage();
    
    // 更新语言切换按钮状态
    updateLanguageButtons();
}

// 更新语言切换按钮状态
function updateLanguageButtons() {
    document.querySelectorAll('[data-lang]').forEach(btn => {
        const lang = btn.getAttribute('data-lang');
        if (lang === currentLang) {
            btn.classList.add('active');
        } else {
            btn.classList.remove('active');
        }
    });
}

// 页面加载完成后初始化
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initLanguage);
} else {
    initLanguage();
}
