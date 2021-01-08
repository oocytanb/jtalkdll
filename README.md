# jtalkdll

これは音声合成システムOpen JTalk使って手軽に音声合成プログラミングするための共有ライブラリです。

## 目次

この文書は以下の順序で記述します。

* 概要
* 動作環境
* フォルダ構成
* ダウンロード
* ビルド
* インストール内容
* 動作確認
* APIの説明
* 利用方法
* 音響モデルデータ、辞書データの指定
* 設定ファイル
* ライセンス

## 概要

PortAudioを使って、マルチプラットフォームでの音声出力を行います。
他のプログラミング言語から使いやすくするために、それぞれの言語のFFI(多言語関数インターフェイス)を利用するコードをいくつか用意しています。
WindowsではC#言語等の.NET Frameworkの言語や、WSH（ウィンドウズスクリプトホスト）で利用しやすいようにC++/CLIでラップしたマネージDLLもオプションで生成します。

このプロジェクトでは、オリジナルのファイル以外に、次に示す他のプロジェクトのファイルを含んでいます。

* [open_jtalk-1.11](http://open-jtalk.sourceforge.net/)
* [open_jtalk_dic_utf_8-1.11](http://open-jtalk.sourceforge.net/)
* [hts_voice_nitech_jp_atr503_m001-1.05](http://open-jtalk.sourceforge.net/)
* [hts_engine_API-1.10](http://hts-engine.sourceforge.net/)
* [htsvoice-tohoku-f01](https://github.com/icn-lab/htsvoice-tohoku-f01)
* [mei (MMDAgent)](http://www.mmdagent.jp/)
* [PortAudio](http://www.portaudio.com/)
* [gradle](https://github.com/gradle/gradle) javaサンプルプログラムにおいて

これらの外部プロジェクトはそれぞれのライセンスに従います。
なお、open_jtalk, hts_engine_APIはスタティックライブラリとして利用しています。
PortAudio はプラットフォームによって、スタティックライブラリまたは共有ライブラリとして利用しています。

### 仕組み

OpenJTalk の open_jtalk.cは、エクスポートしないライブラリの定義にmain関数をくっつけたような構成になっています。
これを利用して、それにwavファイルに含まれる音声データをmallocして返す関数を追加したものを
openjtalk.c とします。
それをラップしてインターフェイス(API)を定義したのが、jtalk.c です。
jtalk.c は内部で portaudio を呼び出すことでマルチプラットフォームでの同期再生、非同期再生を実現しています。
さらに、WindowsではマネージDLLにするために、C++/CLI でラップしているのが jtalkcom.cpp です。 

## 動作環境

以下のプラットフォームで動作確認しています。

* Windows 10
* macOS Catalina
* Ubuntu 19.10 (他のLinuxディストリビューションは未確認)

## ディレクトリ構成

このリポジトリのディレクトリ構成は次の通りです。

```ディレクトリ
├── jtalk     ..... このプロジェクトの本体 jtalkdll ソースファイル
├── hts_engine_API-1.10     ..... hts_engine_API ソースファイル
├── open_jtalk-1.11     ..... open_jtalk ソースファイル（ほんの少し修正）
├── portaudio     ..... portaudioソースファイル
├── voice     ..... 音響モデル用のフォルダ
├── ffi     ..... いろいろな言語から利用するためのインターフェイスファイルとサンプル
└── extra/open_jtalk     ..... open_jtalk だけをビルドするスクリプト
```

## ダウンロード

Windows VC++ によるビルド済みファイルと、それで使用する辞書、音響モデルファイルは、[Releaseページ](https://github.com/rosmarinus/jtalkdll/releases) からダウンロードできます。
これは、展開し C:\open_jtalk として配置させて使うことを前提としています。

## ビルド

コマンドラインで[CMake](https://cmake.org/)を使ってビルドします。
CMakeは、ソフトウェアのビルドを自動化するツールです。
一つの設定ファイルで、LinuxやmacOSだけでなく、WindowsのVC++のビルドも記述できるので、
マルチプラットフォームで共有ライブラリを作る今回のプロジェクトに適しています。

### CMakeLists.txtの設定

このプロジェクトのCMakeLists.txtはコマンドラインでの利用を前提として作られています。
コマンドラインでキャッシュ変数を指定して実行することもできますが、buildスクリプトから呼び出しているので、直接CMakeLists.txtの冒頭を編集する方が分かりやすいことがあります。

冒頭付近に次のようなコメントアウトしたsetコマンドがあり、これを必要に応じてアンコメントすることで設定を変えられます。

```CMake:
#set(build_jtalkcom TRUE)
#set(install_open_jtalk TRUE)
```

1行目は、WindowsのコマンドラインでマネージDLLを作成するときに有効にします。

2行目は、open_jtalk, hts_engine, mecab-dict-index コマンドを一緒にbinフォルダにインストールするときに有効にします。
このjtalkdllを使うときには、open_jtalkそのものは必要ありませんが、動作の確認などに必要ならば、これを有効にしてください。
open_jtalk 用の mecab 辞書をコンパイルするときは、この mecab-dict-index が必要になるので、そのときもこの行をアンコメントしてください。
Windowsでバッチファイルを使ってビルドするときは、変更するのは3行目だけにしてください。

### Windows でのビルド

[マイクロソフト Visual Studio C++](https://www.visualstudio.com/ja/vs/cplusplus/) (以下MSVC)によるビルド方法と、
[MSYS2](http://www.msys2.org/)上の
[MinGW-w64](http://mingw-w64.org/doku.php)のgccコンパイラによるビルド方法を以下に示します。
どちらで作っていいか分からないときは、MSVCでビルドしてください。
ソースのダウンロードの方法によっては、バッチファイルの改行コードがCRLF(Windows)ではなくなっているかもしれません。予め確認してCRLFではないときは、エディタでCRLFで保存しなおしてください。

#### MSVCを使ったビルド

必要なもの：

* Visual Studio 2019
* CMake
* git （必須ではないけど）

まだ、Visual Studio 2019 がインストールされていない場合は、無償の Visual Studio 2019 Comunity　Edition もしくは Build Tools for Visual Studio 2019 をインストールしてください。
どちらも[マイクロソフトのVisual Studioダウンロードサイト](https://www.visualstudio.com/ja/downloads/)からダウンロードができます。
また[Chocolatey](https://chocolatey.org/)を使ってインストールすることもできます。

前者は高機能な統合開発環境、後者はC/C++のコードのコンパイルだけを目的とした最小限の構成のビルドツールですが、
このリポジトリのコンパイルはGUIを必要としない単純な作業なのでビルドツールでも十分です。

インストールが完了したら、``Visual Studio Installer``をスタートメニューから起動して、変更ボタンを押します。
出現したワークロード画面で、このプロジェクトに必要な構成にチェックして、右下の「変更」ボタンを押し、必要なコンポーネントをインストールします。
このとき必要な構成の指定は次の通りです。Visual Studio Community 2019では、ワークロードで「C++によるデスクトップ開発」にチェックします。
Visual Studio Build Tools 2019では、ワークロードで「Visual C++ Build Tools」にチェックします。
COM相互運用のクラスライブラリ``JTalkCOMdll``までビルドするときは、上記の項目の右側オプションで「C++/CLIサポート」にチェックします。

* Build Tools for Visual Studio 2019 のダウンロードボタンの場所が分かりにくいのですが、ページの最後の方の「その他ツール及びフレームワーク」のところにあります。
* COM相互運用のクラスライブラリjtalkCOMx86.dll または jtalkCOMx64.dllを作成するときは、Visual Studio のインストーラを使って C++/CLI サポートをインストールしておきます。
* 将来のバージョンのVisual Studioでもビルド可能か分かりませんが、そのときは後述のコマンドプロンプトを使った方法で試してください。
* [Chocolatey](https://chocolatey.org/)でインストールする場合のパッケージ名は、それぞれ``visualstudio2019community``、``visualstudio2019buildtools``です。

[CMake](https://cmake.org/)がまだインストールされていない場合は、
[ホームページ](https://cmake.org/)からダウンロードし、インストールします。なおインストールオプションの画面でPATHを通すラジオボタンを必ずチェックしてください。手作業で後からPATHを通してもかまいません。

gitは、このリポジトリからソースをコピーするために使いますが、インストールされていなければ、[ZIPファイル](https://github.com/rosmarinus/jtalkdll/archive/master.zip)をダウンロードし展開すればいいので必須ではありません。
gitのインストールは、[Git for Windows](https://git-for-windows.github.io/)からダウンロードして行うか、
[Chocolatey](https://chocolatey.org/)を使ってgitパッケージを``choco inst git``でインストールします。


#### インストール手順

##### コマンドプロンプトを使わない方法

Gitを使うか、[ZIPファイル](https://github.com/rosmarinus/jtalkdll/archive/master.zip)をダウンロードして、このリポジトリのコピーを取得します。
ZIPの場合は適当な場所で展開します。

自分のPCにコピーしたら、jtalkdllフォルダを開いて、build.batを探します。
このバッチファイルをダブルクリックで実行すると、ビルド、そしてインストールします。
これだけで完了です。  
このとき、Windowsのビット数に合わせたjtalkdllなどが生成されます。

システムのビット数とは違うjtalkdllを作るときは、buildx64.bat、buildx86.batを実行します。
なお、32ビットPCでbuildx64.batを実行するとクロスコンパイラで、そのPCでは実行できないjtalkdllを生成します。

このビルドでは、C++/CLI サポートしているかを自動的に判断して、可能ならば、クラスライブラリjtalkCOMx86.dll または jtalkCOMx64.dllを生成します。
ただし生成されるのは署名されていないアセンブリになります。
完全署名されたものが必要なのときは、[Releaseページ](https://github.com/rosmarinus/jtalkdll/releases) からダウンロードしてください。

##### コマンドプロンプトを使う方法

スタートメニューを開き、 Visual Studio 2019 フォルダ内にある 「x64 Native Tools Command Prompt for VS 2019」
あるいは「x86 Native Tools Command Prompt for VS 2019」を起動します。どちらを使うかは64版か32版を作るかどうかで決めます。
開いたら、適当なフォルダをカレントフォルダに決めます。

gitがインストールされているときは次のコマンドを実行します。

```DOS:
git clone https://github.com/rosmarinus/jtalkdll.git
cd jtalkdll
```

gitが無いときは、上記のZIPファイルをダウンロード後、適当なフォルダに展開し、そのjtalkdllフォルダを前述のコマンドプロンプトのカレントフォルダにします。

コピーが完了したら、次の一連のコマンドを実行すると、インストールされます。

```DOS:
cmake .. -G "NMake Makefiles"
nmake
nmake install
```

生成物は c:\open_jtalkフォルダに出力されます。
Windowsで今後jtalk.dllを利用する場合は、システムの詳細設定で環境変数PATHにc:\open_jtalk\binを追加してください。

cmake の行に ``-Dbuild_jtalkcom=true`` を追加すると、クラスライブラリjtalkCOMx86.dll または jtalkCOMx64.dllを生成します。
このクラスライブラリをビルドするためには、Visual Studio のインストーラを使って C++/CLI サポートをインストールしておく必要があります。
ただし生成されるのは、署名されていないアセンブリになります。
完全署名されたものが必要なのときは、[Releaseページ](https://github.com/rosmarinus/jtalkdll/releases) からダウンロードしてください。

cmake の行に ``-Dinstall_open_jtalk=true`` を追加すると open_jtalk.exe, hts_engine.exe, mecab-dict-index.exe をインストールします。

WindowsのCMakeではvcxprojファイルを出力し、MSBuildでビルドする方式もあります。
この方がvcxprojファイルが生成されるので、Visual Studioでのデバッグなどには都合がいいです。
しかし今回はインストールまで自動化させるため nmake.exe を使う方法にしています。

vcxprojを出力して実行する方法は次の通りです。必要な場合は、展開後に以下のコマンドを実行してください。

vs2019で64ビット版をビルドする

```DOS:
mkdir build.dir
cd build.dir
cmake .. -G "Visual Studio 16 2019" -A x64
msbuild ALL_BUILD.vcxproj /p:Configuration=Release;Platform=x64
```

vs2019で32ビット版をビルドする

```DOS:
mkdir build.dir
cd build.dir
cmake .. -G "Visual Studio 16 2019" -A Win32
msbuild ALL_BUILD.vcxproj /p:Configuration=Release;Platform=Win32
```

インストールが完了したら、[動作確認](#validation)を参考に、動作するかどうか確認してみてください。 

#### MSYS2 の MinGW-W64 を使ったビルド

##### MSYS2の準備

* MSYS2のインストール
* 必要なパッケージのインストール

###### MSYS2のインストール

[MSYS2 homepage](http://www.msys2.org/) から、msys2-i686-xxxxxxxx.exe （xxxxxxxxは日付の数列）もしくは msys2-x86_64-xxxxxxxx.exe をダウンロードします。
それぞれ32ビットと64ビット版です。32ビットマシンでない限り、特にこだわりがなければ、64ビット版がいいでしょう。
セットアップの方法は割愛します。

###### 必要なパッケージのインストール

セットアップが終わったら、jtalkdllの生成のためには次のパッケージを追加でインストールします。
git, base-devel, mingw-w64-i686-toolchain, mingw-w64-x86_64-toolchain。
最後の2つは32ビット版、64ビット版のビルドコマンド群なので、両方もしくは必要な方を入れてください。

```bash:
pacman -S git base-devel mingw-w64-i686-toolchain mingw-w64-x86_64-toolchain
```

##### MSYS2でのビルド方法

環境が整ったら、開発用のコンソールを開きます。64bit版を作りたいときは、Windowsのスタート画面からMSYS2 MinGW 64-bit、32bit版ならばMSYS2 MinGW 32-bitです。
そして、カレントフォルダを適当な場所に切り替えて、次の一連のコマンドを実行すると、dllが生成されます。

```bash:
git clone https://github.com/rosmarinus/jtalkdll.git
cd jtalkdll
bash build
```

なお、音声を再生するために利用しているPortAudioがCMakeではうまくmingw用のスタティックライブラリを生成できなかったので、congifureを一箇所改変（後述）して、CMmakeに先だってライブラリを生成させています。

辞書ファイル、音響モデルファイルなどのデータファイルはMSVCのときと同じ場所、c:\open_jtalkフォルダにインストールされます。
jtalk.dllや、実行ファイルのサンプル、ヘッダファイルは、MSYS2のMinGWビルド用のフォルダにインストールされます。
MSYS2の外で、jtalk.dllを使ったプログラムを動かすためには、そのプログラムと同じフォルダにjtalk.dllを配置するか、
jtalk.dllを手作業でc:\open_jtalk\binにコピーして、ここにPATHを通すかしてください。

buildスクリプト中のcmake の行に ``-Dinstall_open_jtalk=true`` を追加すると open_jtalk、hts_engine、mecab-dict-index をインストールします。

インストールが完了したら、[動作確認](#validation)を参考に、動作するかどうか確認してみてください。 

##### PortAudio でスタティックライブラリを作る改変

今回、MingGWではCMake中でスタティックライブラリをリンクできず、またportaudioのconfigureを使ったビルドでもスタティックライブラリそのものが作れませんでした。
試行錯誤でconfigureの15194行にあるSHARED_FLAGSの -shared オプションが邪魔をしているようなので、これを削除して、スタティックライブラリを作っています。


### macOS でのビルド

#### 準備

cmake が必要です。

そのために、まずコマンドライン・デベロッパ・ツールをインストールします。
これは cmake をインストールするために使うHomebrewに必要だからです。

```bash:
xcode-select --install
```

次に、macOS用のパッケージマネージャーHomebrewをインストールします。
これを使わないcmakeをインストール方法もありますが、今回はこれを使います。
インストールの仕方と、使い方は、[Homebrew](https://brew.sh/index_ja.html)にあります。

ようやく、cmakeのインストールです。

```bash:
brew install cmake
```

#### macOSでのビルド方法

以下の一連のコマンドを入力します。
ビルドが終了し、インストールが始まると、管理者パスワードが求められます。

```bash:
git clone https://github.com/rosmarinus/jtalkdll.git
cd jtalkdll
bash build
```

インストールが完了したら、[動作確認](#validation)を参考に、動作するかどうか確認してみてください。 

### Ubuntu でのビルド

以下に、Ubuntuでのビルド方法を示します。

他のlinuxディストリビューションでも同様の処理を行えば、生成できると思われますが、Ubuntuを例として示します。
libconvに関しては簡単な存在判定ルーチンを入れて自動処理しています。
万が一これがうなく働かないときは、CMakeFile.txtに手を加えて対処してください。

#### Ubuntuでの準備

まず、C/C++などの開発環境が整っていないときは、次のコマンドでインストールしておいてください。
```bash:
sudo apt install build-essential cmake git
```

portaudio で必要になるので、次のコマンドでALSAのSDKをインストールしておきます。

```bash:
sudo apt install libasound-dev
```

PortAudioパッケージがそのディストリビューション向けに用意されているときは、jtalkdllをビルドする前に、できればそれをインストールしておいてください。
分からないときは、そのままでもかまいません。
CMakeを実行すると、PortAudioのライブラリを探索し見つからなければ、自動的に同梱しているPortAudioのソースからビルドします。

あとは次の一連のコマンドで、ソースファイルをコピーして、ビルドします。
ビルドが終了し、インストールが始まると、管理者パスワードが求められます。

```bash:
git clone https://github.com/rosmarinus/jtalkdll.git
cd jtalkdll
bash build
```

インストールが完了したら、[動作確認](#validation)を参考に、動作するかどうか確認してみてください。 

## インストール内容

ここでは上記のビルドによって、インストールされるファイルについて述べます。

### インストールされるファイル

このプロジェクトをビルドすることによって、以下の成果物とデータが配置されます。

#### 共有ライブラリ

* jtalk.dll / libjtalk.so / libjtalk.dylib

#### COM 相互運用クラスライブラリ

MSVCのみ。署名なし。通常コマンドラインからのビルドではビルド無効。  
完全署名されたものが必要なのときは、[Releaseページ](https://github.com/rosmarinus/jtalkdll/releases) からダウンロードしてください。

* jtalkCOMx86.exe, jtalkCOMx64.exe
* COMを登録・解除するためのバッチファイル regist_jtalkcom.bat, unregist_jtalkcom.bat

#### ヘッダファイル

* jtalk.h

#### インポートライブラリ

MSVCのみ

* jtalk.lib

#### サンプルプログラム

* jtd_c.exe / jtd_c
* jsay.exe / jsay

#### データファイル

* 音響モデルファイル
* 辞書データ

### インストール先

ビルドによる成果物とデータは、プラットフォームによって以下のフォルダにインストールされます。

#### Windows MSVC での配置先

* 共有ライブラリ jtalk.dll ... c:\open_jtalk\bin
* COM 相互運用クラスライブラリ jtalkCOMx64.dll, jtalkCOMx86.dll ... c:\open_jtalk\bin
* ヘッダファイル jtalk.h ... c:\open_jtalk\include
* インポートライブラリ jtalk.lib ... c:\open_jtalk\lib
* サンプルプログラム jtd_c.exe, jsay.exe ... c:\open_jtalk\bin
* MeCab辞書ファイル ... c:\open_jtalk\dic_utf_8
* 音響モデルファイル ... c:\open_jtalk\voice
* バッチファイル regist_jtalkcom.bat, unregist_jtalkcom.bat ... c:\open_jtalk\bin


#### Windows MSYS2 での配置先

以下に出てくる環境変数 MINGW_PREFIX の値は mingw32 または mingw64 である。

* 共有ライブラリ jtalk.dll ... C:\open_jtalk\bin, /$MINGW_PREFIX/bin
* ヘッダファイル jtalk.h ... C:\open_jtalk\bin, /$MINGW_PREFIX/include
* サンプルプログラム jtd_c, jsay ... C:\open_jtalk\bin, /$MINGW_PREFIX/bin
* MeCab辞書ファイル ... c:\open_jtalk\dic_utf_8
* 音響モデルファイル ... c:\open_jtalk\voice

#### macOS / Ubuntu での配置先

* 共有ライブラリ libjtalk.dylib / libjtalk.so ... /usr/local/bin
* ヘッダファイル jtalk.h ... /usr/local/include
* サンプルプログラム jtd_c, jsay ... /usr/local/bin
* MeCab辞書ファイル ... /usr/local/OpenJTalk/dic_utf_8
* 音響モデルファイル ... /usr/local/OpenJTalk/voice

### インストール先の変更

上記のように、MSVC、MinGWでの標準のインストール先は、実行ファイル、dllは``C:\open_jtalk\bin\``、
データフォルダは``C:\open_jtalk\``です。
macOS、Linuxでの標準のインストール先は、実行ファイル、ライブラリは``/usr/local/bin/``、
データフォルダは``/usr/local/OpenJTalk/``です。

変更するには、次のキャッシュ変数をCMakeLists.txtの冒頭で定義しておきます。
実行ファイルのインストール先は``BIN_INSTALL_PREFIX``のbin、
データフォルダのインストール先は``DATA_INSTALL_PREFIX``です。


## <a name="validation">動作確認</a>

対象のプラットフォームにおいて上記の方法でビルドが成功したら、以下の方法で動作確認ができます。

次のコマンドをタイプしエンターしてください。うまくビルドできていれば、一緒にインストールしてあるhtsvoiceファイルをランダムに選んで言葉をしゃべってくれます。
Windowsの場合は、MinGWを含め、インストール先をWindowsのコマンドプロンプトでカレントフォルダにするか、エクスプローラで開いてマウスでダブルクリックするかして、実行してください。

```bash:
jtd_c
```

このコマンドのソースはjtalkフォルダにある[jtd_c.c](https://github.com/rosmarinus/jtalkdll/blob/master/jtalk/jtd_c.c)で、C言語で書いたこの共有ライブラリのサンプルコードです。

次はもう一つのサンプルの[jsay](https://github.com/rosmarinus/jtalkdll/blob/master/jtalk/jsay.c))です。
下のコマンドラインのように パラメータに日本語を入力して、エンターしてください。うまくいけば、その言葉をしゃべります。

```bash:
jsay -v mei_normal こんにちは
```

Windowsの場合（MinGWでビルドしても）は、UTF-8エンコードで書かれたテキストを、jsay.exe のアイコンにマウスでドロップしても確認できます。

jsayはmacOSのsayコマンドに動作を似せたサンプルで、このソースもjtalkフォルダにあります。
-v?オプションで利用できる音声のリストが出ますので、それを参考にして音声を指定してください。
-oファイル名 オプションでファイルへの書き出しもできます。
文字列が指定されていないと、標準入力からの入力を待ちます。
エンターを連続して入力するまで続きます。

## APIの説明

jtalk.cではopen_jtalkを利用するのに便利な関数をAPI関数としていくつか定義しています。
ここで定義されているAPI関数には接頭語'openjtalk_'をつけています。

本来の open_jtalk で指定できるパラメータの設定と取得ができます。
設定はset、取得はgetの文字列が名前に含まれます。
パラメータの名前はコマンドラインで指定する短い名前とその意味を反映した長い名前の両方を用意しています。

文字列を入出力するAPIにおける文字コードは基本的にUTF-8です。内部で使っているmecabにもUTF-8文字列を渡しています。
しかし、プログラミング言語によっては16ビットのUnicodeを文字コードとして使っているものもあり、受け渡しが簡単になるようにUTF-16LEの文字列のを引数、戻り値になる関数も用意しています。これらには名前の末尾にU16が付いています。
また Windowsではコマンドプロンプトの出力などShift-JIS（CP932）を使わざるを得ない場合もあるので、Shift-JISも関数の引数、戻り値として使えるようにしています。こちらには名前の末尾にSjisが付いています。

Utf-16leを扱う文字の型はchar16_t、文字列の型はchar16_t*を使っています。
Windowsの wchar_t は厳密には同じ型ではありませんが、内部では同じものとして使っています。
サロゲートペア文字列などへの対応は今後の課題とします。

### 主要なAPI

API関数についての詳しい内容は [jtalk.h](https://github.com/rosmarinus/jtalkdll/blob/master/jtalk/jtalk.h) を直接見てください。

この中で主要なものを抜き出してみます。

```C:
// 初期化
OpenJTalk *openjtalk_initialize(const char *voice, const char *dic, const char *voiceDir);

// 後始末
void openjtalk_clear(OpenJTalk *oj);

// サンプリング周波数
void openjtalk_setSamplingFrequency(OpenJTalk *oj, unsigned int i);
unsigned int openjtalk_getSamplingFrequency(OpenJTalk *oj);


// 音響モデルファイル
//  絶対パス...直接、相対パス...実行ファイルの位置を基準、名前のみ...探索
bool openjtalk_setVoice(OpenJTalk *oj, const char *path);

// 現在の音響モデルファイルの取得
// pathは確実に長さMAX_PATHの配列
char *openjtalk_getVoice(OpenJTalk *oj, char *path);


// 同期発声。完了するまでこの関数から戻らない。
void openjtalk_speakSync(OpenJTalk *oj, const char *text);

// 非同期発声。発声を開始したらこの関数から戻る。
// この関数から戻った後は、次の関数によって音声の操作および状態の取得、待機を行う。
// openjtalk_pause、openjtalk_resume、openjtalk_stop、openjtalk_isSpeaking、
// openjtalk_isPaused、openjtalk_isFinished、openjtalk_waitUntilDone、openjtalk_waitUntilFinished、
// openjtalk_wait(0)
void openjtalk_speakAsync(OpenJTalk *oj, const char *text);

// 非同期発声を一時停止する。この一時停止はopenjtalk_resumeによってのみ再開される。
// 一時停止中の再度の一時停止は何もしない。
// 発声の停止が行われると、一時停止は無効となり、発声は完了する。
// 同期・非同期発声関数が呼び出されると、それが実行される前に、一時停止されていた発声は完了する。
void openjtalk_pause(OpenJTalk *oj);

// 非同期発声の一時停止からの再開。停止位置からの音声変換の再開ではなく音声データの再生再開。
void openjtalk_resume(OpenJTalk *oj);

// 非同期発声の強制停止。一時停止中の場合は、再開は無効となる。
void openjtalk_stop(OpenJTalk *oj);

// 非同期発声が発声中かどうか（一時停止中は偽）
// 一時停止の可能性がないときは、openjtalk_isFinishedの否定。
bool openjtalk_isSpeaking(OpenJTalk *oj);

// 一時停止中かどうか
bool openjtalk_isPaused(OpenJTalk *oj);

// 非同期発声が完了するまで待機する
void openjtalk_waitUntilDone(OpenJTalk *oj);
```

基本的な利用法は次のようになります。

* openjtalk_initialize でOpenJTalk構造体を確保します。このとき、標準の音響モデル、標準の辞書データ、標準の音響モデルフォルダが設定されます。同時に標準のパラメータも設定されます。
* openjtalk_initializeの戻り値であるOpenJTalk構造体へのポインタをハンドルにして、openjtalk_setVoice や openjtalk_setSamplingFrequency などを呼び出し、値を調整します。ただし、音響モデルファイルを変更すると、それに合わせてパラメータが初期化されるので、openjtalk_setVoiceの後に、パラメータの設定の関数を呼び出すようにします。
* そして、openjtalk_speakAsync もしくは openjtalk_speakSyncに文字列を渡してしゃべらせます。
* 発声が全て終わったら、構造体を解放するために、openjtalk_clear を呼び出します。

単純な使用例をC言語で書くと次のようになります。

```C:hello.c
#include "jtalk.h"

void say(char*message)
{
  OpenJTalk *oj = openjtalk_initialize("","","");
  openjtalk_speakSync(oj,message);
  openjtalk_clear(oj);
}

int main()
{
  say(u8"こんにちは、世界");
  return 0;
}
```

## 利用方法

### jtalk共有ライブラリ の C言語からの利用

jtalk共有ライブラリを使ったC言語でプログラミングの仕方を説明します。
一般的な共有ライブラリを使ったプログラミングの仕方が分かれば、問題ありません。
C言語でライブラリを使ったプログラミングをするには、ライブラリとそのライブラリで定義されている関数を記述したインクルードファイルが必要になります。
正常にビルドが完了していれば、ライブラリとインクルードファイルは所定の場所に配置されているはずです。

#### gcc による利用

gccやclangの場合は共有ライブラリ名をコマンドラインに-lオプションで指定します。
例えば、ubuntuのgccの場合は次のようになります。

```bash:
gcc hello.c -ljtalk -ohello
```

#### Windows MSYS2 MinGW-w64 gcc.exe による利用

MSYS2上のMinGW-w64は、MSYS2 MinGW 32-bitとMSYS2 MinGW 64-bitの、どちらのコンソールを開くかによって環境を区別します。

```bash:
gcc hello.c -L$MINGW_PREFIX/bin -ljtalk -ohello
```

環境変数MINGW_PREFIXには、コンソールに応じてmingw64かmingw32の文字列が入っているので、これを使ってjtalk.dllの位置を指定しています。
同じMSYS2 MinGWコンソール内では jtalk.dll にパスが通っているので、そのまま実行できます。
通常のWindowsアプリケーションと同じように実行するには、同じアーキテクチャでビルドしたjtalk.dllが、同じフォルダかパスが通ったところに存在する必要があります。

#### Windows cl.exe による利用

Windowsのcl.exeの場合は、共有ライブラリそのものではなく、インポートライブラリをコマンドラインに指定します。
このインポートライブラリはc:\open_jtalk\libにあります。
ここにあるライブラリはコンパイルしたアーキテクチャーによって名前が区別されています。
具体的には、x64でコンパイルしたものは jtalkx64.libで、x86はjtalkx86.libです。
利用するコンパイラのアーキテクチャに従ってこのライブラリを選びます。

例えば、x64版cl.exeで上記のhello.cをコンパイルするコマンドラインはこうになります。
このとき適切なVSコマンドプロンプトで開いているものとします。

```DOS:
set JTALKDIR=c:\open_jtalk
cl /I %JTALKDIR%\include hello.c jtalkx64.lib /link /LIBPATH:%JTALKDIR%\lib
```

もちろん、できあがった hello.exe を実行するときは、同じアーキテクチャでビルドしたjtalk.dllが、同じフォルダかパスが通ったところに存在する必要があります。

### 他のプログラミング言語からの利用

この共有ライブラリを他のプログラミング言語から利用するためのサンプルプログラムをffiフォルダの中に集めています。
これらの名前には'jtd_'の接頭辞を付けています。

サンプルでは、各言語のFFI(Foreign function interface)機能を利用しています。
各言語との仲介をするファイルの名前は、

``jtalk.<言語の拡張子>``

という形式になっています。例えば、luajit用は``jtalk.lua``です。

現在(2020.2.11)対応している言語は、
[C++Builder](https://www.embarcadero.com/jp/products/cbuilder/starter)、
C++、
C++/CLI、
C#、
[D](https://dlang.org/)、
[Delphi](https://www.embarcadero.com/jp/products/delphi/starter)、
[Java](https://www.java.com/ja/) (
  [Groovy](http://groovy-lang.org/index.html)、
  [Kotlin](https://kotlinlang.org/)、
  [Scala](https://www.scala-lang.org/)
)、
[Julia](https://julialang.org/)、 
[LuaJIT](http://luajit.org/)、
[node.js](https://nodejs.org/ja/)、
[Objective-C](https://developer.apple.com/jp/xcode/)、
[Python](https://www.python.org/)、
[Ruby](https://www.ruby-lang.org/ja/)、
[Rust](https://www.rust-lang.org/ja/)、
[Swift](https://www.apple.com/jp/swift/)、
VisualBASIC、
WSH(
  JScript、
  VBScript
)
です。
C++からの利用するためのヘッダファイルとサンプルプログラムも、ffiフォルダに配置しています。
一部のプラットフォーム限定の言語もありますが、できる限りマルチプラットフォームで実行できるようにしています。
Objective-CとSwiftは、現在macOSでのみ動作します。

それぞれの言語には、上記のjtd_cと同等のコンソールプログラムを用意しています。

それ以外にいくつかウィンドウを表示するGUIのサンプルもあります。
[ffi/cbuilder/jtd_cb.cbproj](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/cbuilder/jtd_cb.cbproj), 
[ffi/cpp/jtd_cppqt.cpp](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/cpp/jtd_cppqt.cpp), 
[ffi/cppcli/jtd_cli.cpp](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/cppcli/jtd_cli.cpp), 
[ffi/cppcli/jtd_clim.cpp](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/cppcli/jtd_clim.cpp), 
[ffi/csharp/jtd_cs.cs](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/csharp/jtd_cs.cs), 
[ffi/csharp/jtd_csm.cs](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/csharp/jtd_csm.cs), 
[ffi/delphi/jtd_delphi.dproj](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/delphi/jtd_delphi.dproj), 
[ffi/java/javaSwingSample/src/main/java/JtdJnaJavaSwing.java](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/java/javaSwingSample/src/main/java/JtdJnaJavaSwing.java), 
[ffi/java/javaFXSample/src/main/java/JavaFXSample.java](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/java/javaFXSample/src/main/java/JavaFXSample.java), 
[ffi/python/jtd_qt5.py](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/python/jtd_qt5.py), 
[ffi/vb/jtd_vb.vb](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/vb/jtd_vb.vb), 
[ffi/vb/jtd_vbm.vb](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/vb/jtd_vbm.vb)
はGUIプログラムです。

またサンプルの名前には次のような規則で区別しています。
CUIとGUIかどうかで対立するサンプルがあるときは、CUIの方に接尾辞'c'を付けています。
アンマネージDLLかマネージDLLを使うかで対立するサンプルがあるときは、マネージDLLを使う方に接尾辞'm'を付けています。

それぞれの言語は、できるだけ最新の言語環境を構築して実行してください。
* 例えば Ubuntu の aptでインストールされる julia では is_unix や unsafe_string などが使えない古いものなので(2017/11/1時点)、
* julia で使うには、ホームページから v1.31以降をダウンロードして利用してください。
* それから、ffi/cpp/jtd_cppqt.cpp, ffi/python/jtd_qt5.pyは[QT5](https://www.qt.io/)を用いたサンプルです。
* ビルド・実行には言語環境の他に、[Qt](https://www1.qt.io/download-open-source/)が必要になります。
* また、JavaではC言語のDLLを利用するために[jna](https://github.com/java-native-access/jna)を利用しています。
* node.jsではnode-ffi-napiモジュールを利用しています。しかし、現時点(2020.2.11)でこれ自体が環境によってうまくインストールできません。
* rustの実行ファイルをWindowsでビルドする時は、jtalk.libにリネームしたインポートライブラリを適切な場所に配置してください。

ビルドする手順が複雑なものには、接頭辞'build_'を付けたスクリプトを用意しています。
これらのスクリプト内のフォルダ名やバージョン番号は環境に合わせて適宜書き換えてください。
単にコンパイラやインタプリタの引数にすればいいものには実行スクリプトは用意していません。

#### 他の言語からの利用例

##### LuaJIT での利用例

ここではLuaJITでの例を紹介します。

[LuaJIT](http://luajit.org/)は、[本家Lua](https://www.lua.org/)との大きな違いとして、
標準内蔵されているFFI機能があります。
これを利用して簡単に共有ライブラリを利用するLuaプログラムを書くことができます。

C言語による先の例をLuaJITで書くと次のようになります。

```lua:hello0.lua
ffi = require("ffi")
jt = ffi.load("jtalk")
ffi.cdef [[
void *openjtalk_initialize(const char *voice, const char *dic, const char *voiceDir);
void openjtalk_speakSync(void *oj, const char *text);
void openjtalk_clear(void *oj);
]]
function say(message)
  local handle = jt.openjtalk_initialize("","","")
  jt.openjtalk_speakSync(handle, message)
  jt.openjtalk_clear(handle)
end
say("こんにちは、世界")
```

その都度C言語での定義を書いてもいいですが、
C言語の関数をluaの関数でラップして、モジュールとしてまとめたものを用意しています。
これは [ffi/luajit/jtalk.lua](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/luajit/jtalk.lua) に置いてあります。
jtalk.luaを使うと、このようになります。

```lua:hello.lua
function say(message)
  local tts = require("jtalk").new()
  tts:speakSync(message)
  tts:destroy()
end
say("こんにちは、世界")
```

##### Java VM からの利用例

jtalkdllをJavaから使うために、簡単にJavaから共有ライブラリを使うことができる[JNA](https://github.com/java-native-access/jna)ライブラリを利用しています。
API関数をラップしてJavaおよびJavaVM言語から利用しやすい形にしたのが、[JTalkJna.java](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/luajit/jtalk.lua) です。
この JTalkJna.java を``build_jtalk_jar``スクリプトで単純なjarファイルにした jtalk.jar を import して使います。
残念ながら、jtalk.jar は JavaSpeechAPI の実装ではありません。

Java関連のプログラムは、gradleでビルドするようにしています。ローカルに配置した後、ffi/java/に移動して、``./gradlew build``とすると、jarの生成とすべてのサンプルをコンパイルします。
もちろんJavaの開発環境を構築しておく必要があります。例えば、Windowsだと前述のchocolateyを使って、``choco install openjdk``などとして。
サンプルを実行するには、``./gradlew javaSample:run``とします。
windowsの場合は、この実行前に 最新のjtalk.dllをffi/java/に置いておき、``./gradlew javaSample:copydll``でコピーしてから実行します。
サンプルのプロジェクトは他に、kotlinSample、groovySample、scalaSample、javaSwingSample、javaFXSample があります。

JTalkJnaの内容は、ダウンロードして、
[JTalkJna-JavaDoc](http://htmlpreview.github.io/?https://github.com/rosmarinus/jtalkdll/blob/master/ffi/java/javadoc/index.html)
を見てください。

ここでは、Java ではなく、[Kotlin](http://kotlinlang.org/) で例を示します。

```Kotlin:Hello.kt
import com.github.rosmarinus.jtalk.JTalkJna
fun say(message:String) {
  var tts = JTalkJna()
  tts.voiceName = "mei_happy"
  tts.speakAsync(message)
  while(tts.isSpeaking);
}
fun main(args:Array<String>) {
  say("こんにちは、世界")
}
```

なお、OpenJTalk には Java で書かれたクローンである [Gyutan](https://github.com/icn-lab/Gyutan) があります。
本格的に Java で OpenJTalk の音声合成技術を使う場合は、こちらを使った方がいいでしょう。

こんな感じで他の言語も書いていけます。
ただし言語によってはffiモジュールをインストールする手間が必要になります。
各言語における共有ライブラリの参照の仕方さえ分かれば、ここで用意していないプログラミング言語でも利用できるはずです。

### JTalkCOM の利用

JTalkCOMx86.dll と JTalkCOMx64.dll を使った Windowsでのプログラミングの方法を示します。

#### マネージDLL としての利用

JTalkCOMは、Windows の C++/CLIのcl.exe, C#のcsc.exe, VisualBASICのvb.exe などで利用できます。
サンプルは 
[ffi/clicpp](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/clicpp)、
[ffi/csharp](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/csharp)、
[ffi/vb](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/vb) 
 の中の名前の末尾が``m``もしくは``mc``のものです。

C#での例を示します。

```C#:hello.cs
using JTalkCom;
using System;

namespace JTalkSample
{
    public class MainClass
    {
        static void say(string message)
        {
            using (var tts = new JTalkTTS {})
            {
                tts.SpeakSync(message);
            }
        }

        [STAThread]
        static void Main()
        {
          say("こんにちは、世界");
        }
    }
}
```

ビルドするためのコマンドラインは次のようになります。
jtalkdllのインストール時に示したVSコマンドプロンプトを使用します。

変数を使わないで64ビット向けを書くと：

```DOS:
rem csc /platform:x64 /target:exe /reference:C:\open_jtalk\bin\JTalkCOMx64.dll hello.cs
```

変数を使って汎用性をもたせると:

```DOS:
set JTALKDIR=c:\open_jtalk
set jtalkcom=JTalkCOM%VSCMD_ARG_HOST_ARCH%.dll
copy %JTALKDIR%\bin\%jtalkcom% .
csc /platform:%VSCMD_ARG_HOST_ARCH% /target:exe /reference:%jtalkcom% hello.cs
```

clやvbで使う場合は、対応する記述に書き換えます。
[ffi/clicpp](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/clicpp)、
[ffi/csharp](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/csharp)、
[ffi/vb](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/vb) の例を見比べると分かりやすいでしょう。
同様にコマンドラインでのビルドオプションの違いは、サンプル付属のビルドスクリプトを確認ください。
VisualStudioのIDEを使う場合は、アセンブリの参照にjtalkCOMx64.dllかjtalkCOMx86.dllを追加します。

なお、これと同様な処理は共有ライブラリ jtalkdll を P/Invoke（プラットフォーム呼び出し）を使っても実現できます。
この方法だと、mono を使ってC#のコードを Windows以外でも実行できます。
[ffi/csharp/jtalk.cs](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/clicpp/jtalk.cs)を使用します。
jtalk.csを使うときは上記のコード冒頭の``using JTalkCom;``を``using JTalkDLL;``に書き換えます。

#### COMオブジェクトとしての利用

JTalkComはC++/CLIで書かれたマネージDLLですが、COM相互運用機能によりCOMオブジェクトとして振る舞います。
このDLLの名前の由来でもあります。

まず、COMへの登録は``regist_jtalkcom.bat``を管理者権限で実行して行います。
内部でregasm.exeコマンドを呼び出しています。
管理者権限での実行は、エクスプローラ上でこのファイルを右クリックして、コンテキストメニューを出し、
そのメニューの中の「管理者として実行」を選んで行います。
ビルド版は署名が行われていないので、いろいろメッセージが出るかもしれません。
解除するときは``unregist_jtalkcom.bat``を管理者権限で実行します。

JScriptのファイルの拡張子である``.js``は他のアプリケーションに関連付けされていることが多いので、同等なXML形式の``.wsf``ファイルで記述します。

```XML:hello.wsf
<?xml version="1.0" encoding="utf-8" ?>
<job id="hello"><script language="JScript">
<![CDATA[

var say=function(message){
  var tts = new ActiveXObject("JTalk.TTS");
  tts.SpeakSync(message);
}
say("こんにちは、世界");

]]>
</script></job>
```

これをUTF-8エンコードでhello.wsfという名前を付けて保存して、そのファイルのアイコンをダブルクリックすれば、しゃべります。
サンプルコードとして、[ffi/wsf/jtd_js.wsf](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/wsf/jtd_js.wsf)、
[ff/wsf/jtd/jtd_vbs.wsf](https://github.com/rosmarinus/jtalkdll/blob/master/ffi/wsf/jtd_vbs.wsf) を用意しています。

## 音響モデルデータ、辞書データの指定

jtalkdllを利用するプログラムは、音響モデルデータと辞書データが必要になります。
これまで出てきたコードも、ほぼ暗黙的にそれらのデータを使ってきたことになります。

ここでは、これらのデータの指定方法、指定が省略されたときの処理などについて詳しく記述します。

### データの位置

標準の辞書フォルダはインストールフォルダの``dic_utf_8``フォルダ、音響モデルデータはインストールフォルダの``voice``フォルダに配置されています。

なお、インストールフォルダは、Windowsでは``c:\open_jtalk``、Windows以外では``/usr/local/OpenJTalk/``です。
これはCMakeLists.txtで定義しています。

これらの標準データを暗黙的に利用、もしくは明示的に指定します。また標準データ以外のものも指定できます。

音声を追加する場合は、標準音響モデルフォルダの中に入れると、関数 openjtalk_getHTSVoiceList によって取得できるリストに入り、
またopenjtalk_setVoiceにおいて名前のみで音響モデルを指定できるようになるので管理がしやすくなります。
標準フォルダの中に入れなくても、初期化関数``openjtalk_initialize``の第1引数で指定したり、
設定ファイルを使って標準音声として指定できます。設定ファイルの表記法については後述します。

辞書ファイルも標準とは別のものを使うことができます。
初期化関数``openjtalk_initialize``の第2引数で指定したり、設定ファイルに書いたり、プログラムの途中で``openjtalk_setDic``などを使って指定します。

### プログラムからの指定方法

#### 暗黙的利用

いままで示したサンプルコードのように、インストールが成功しさえすれば、詳しい内容を知らなくても暗黙的に利用できます。
最低限、データの指定はこの二つの関数の使用例を覚えておけば使えます。

初期化関数で全ての引数を``NULL``または``""``にすると、設定ファイルか、標準設定を利用することを意味します。

```C:
  // 音響モデルフォルダ、辞書フォルダ、音響モデルファイルを標準設定にする。
  OpenJTalk *oj = openjtalk_initialize("","","");
```

現在の音響モデルフォルダにある音響モデルファイルは、拡張子のない名前で指定できます。

```C:
  // 現在の音響モデルをmei_normal.htsvoiceに変更する。
  openjtalk_setVoice("mei_nomral");
```

#### 初期化時の指定

初期化関数``openjtalk_initialize``が呼び出されると、音響モデルフォルダ、辞書フォルダ、音響モデルファイルが特定されます。
再初期化関数``openjtalk_refresh``が呼び出されたときも、リセット後、保存していた``openjtalk_initialize``の引数を再評価し、内部では同じ処理が呼び出されます。

openjtalk_initializeの処理中、音響モデルフォルダ、辞書フォルダ、音響モデルファイルの特定は以下の規則によって決められます。
一見、複雑に見えますが、実行ファイルの近くにあるものを優先し、Windowsはその次にdllの近く、
そして見つからなければインストールしたときの標準データを使うということを示しています。

openjtalk_initializeの引数voice、dic、voiceDirは、値が``NULL``または``""``のとき、省略を意味します。
値を空にする意味ではありません。

以下の工程を経ても辞書フォルダ、音響モデルファイルの2つのいずれかが決定できないときは、初期化が失敗します。

##### voiceDir 音響モデルフォルダの優先順位

1. 引数による指定
1. カレントフォルダの設定ファイルによる指定
1. カレントフォルダの音響モデルフォルダ
1. 親フォルダの音響モデルフォルダ
1. (win) 共有ライブラリのあるフォルダの設定ファイルによる指定（カレントフォルダに設定ファイルがないとき）
1. (win) 共有ライブラリのあるフォルダの音響モデルフォルダ
1. (win) 共有ライブラリの親フォルダの音響モデルフォルダ
1. 標準の音響モデルフォルダ（インストールフォルダの音響モデルフォルダ）

音響モデルフォルダの名前は``voice``フォルダ、``voice``で始まるフォルダ、``hts_voice``で始まるフォルダの順で探す。  
冒頭に``(win)``とある項目はWindows限定。

##### dic 辞書フォルダの優先順位

1. 引数による指定
1. カレントフォルダの設定ファイルによる指定
1. カレントフォルダの辞書フォルダ
1. 親フォルダの辞書フォルダ
1. (win) 共有ライブラリのあるフォルダの設定ファイルによる指定（カレントフォルダに設定ファイルがないとき）
1. (win) 共有ライブラリのあるフォルダの辞書フォルダ
1. (win) 共有ライブラリの親フォルダの辞書フォルダ
1. 標準の辞書フォルダ（インストールフォルダの辞書フォルダ）

辞書フォルダの名前は``dic_utf_8``フォルダ、``dic``で始まるフォルダ、``open_jtalk_dic_utf_8-``で始まるフォルダの順で探す。  
冒頭に``(win)``とある項目はWindows限定。

このとき辞書データがUTF-8エンコード向けかどうか、
unk.dic の先頭から40バイト目の文字列で簡単な確認が行われます。
エンコードが違えば、そのフォルダに辞書は存在しないと見なします。

##### voice 音響モデルファイルの優先順位

1. 引数による指定
1. カレントフォルダの設定ファイルによる指定
1. (win) 共有ライブラリのあるフォルダの設定ファイルによる指定（カレントフォルダに設定ファイルがないとき）
1. 現在の音響モデルフォルダ内の``mei_normal.htsvoice``
1. 現在の音響モデルフォルダで最初に見つかった音響モデルファイル

#### voiceDir, dic, voice 指定文字列の内容

``voiceDir``は、絶対パスと相対パスによって音響モデルフォルダの位置を指定します。
相対パスは設定ファイルに記述されている場合は、設定ファイルのある場所からの相対位置になります。
引数に相対パスが書かれていた場合は、実行ファイルからの相対位置になります。
絶対パスか相対パスによる指定でそこにフォルダが確かに存在すれば、そのフォルダが現在の音響モデルフォルダに設定されます。

``dic``は、絶対パスと相対パスによって辞書フォルダの位置を指定します。
相対パスは設定ファイルに記述されている場合は、設定ファイルのある場所からの相対位置になります。
引数に相対パスが書かれていた場合は、実行ファイルからの相対位置になります。
絶対パスか相対パスによる指定でそこにフォルダが確かに存在すれば、そのフォルダが現在の辞書フォルダに設定されます。
このとき辞書データがUTF-8エンコード向けかどうかの確認が行われます。

``voice``は、絶対パスと相対パス、そして名前によって音響モデルの位置を指定します。
相対パスは設定ファイルに記述されている場合は、設定ファイルのある場所からの相対位置になります。
引数に相対パスが書かれていた場合は、実行ファイルからの相対位置になります。
絶対パスか相対パスによる指定でそこにファイルが確かに存在すれば、そのファイルが現在の音響モデルに設定されます。
存在しなければ、現在の音響モデルフォルダの中の任意の音響モデルファイルが設定されます。
名前による指定は、現在の音響モデルフォルダ内での再帰探索の指定です。
適合するものが複数あったとしても最初に見つかったもののみが選ばれます。
名前は拡張子``.htsvoice``を除いた部分です。``*``、``?``のワイルドカード文字も使えます。
対象の音響モデルフォルダ内のファイルが多すぎるときは、探索が途中で終了します。
具体的には``VOICESEARCHMAX``(=200)個のファイルを調べても見つからないときは、存在しないと見なされます。

#### 初期化の後の変更

openjtalk_initialize 内で、暗黙的・明示的に確定した音響モデルフォルダ、辞書フォルダ、音響モデルファイルの値は、他の関数を使ってプログラム内で変更できます。

主な関数は次の通りです。

```C:
bool openjtalk_setDic(OpenJTalk *oj, const char *path);
bool openjtalk_setVoiceDir(OpenJTalk *oj, const char *path);
bool openjtalk_setVoice(OpenJTalk *oj, const char *path);
```

``openjtalk_setDic``と``openjtalk_setVoiceDir``の引数``path``は上記と同じように絶対パス、相対パスを指定できます。
``openjtalk_setVoice``はそれに加えて名前による指定ができます。

これらの関数ではいくつか注意することがあります。

* 関数``openjtalk_setVoice``を使って、現在の音響モデルファイルを変更すると、それに伴って音響モデルのパラメータが初期化されます。
* 関数``openjtalk_setVoiceDir``を使って、現在の音響モデルフォルダを変更すると、現在の音響モデルファイルも変更される可能性があります。現在の音響モデルファイルが新しい音響モデルフォルダ内のファイルでもあるならば、そのままです。しかし、新しい音響モデルフォルダに含まれないならば、新しいフォルダの中に同じ名前のファイルを探して、それを現在の音響モデルファイルにします。さらになければ、標準音声``mei_normal``を探し、それも見つからなければ、最初に見つかった音声を設定します。これは先だって絶対パスや相対パスで音響モデルが指定されていた場合も例外ではありません。
* 指定のファイルやフォルダが見つからないとき、初期設定のときと違って、設定ファイルによる指定は反映されません。

## 設定ファイル

jtalkdllでは設定ファイルを使って、ファイルやフォルダのパスの初期値を指定することができます。
あまり役に立たないかもしれませんが、音響モデルのパラメータの初期値も指定できます。

### 設定ファイルの配置

#### 名前と配置

この設定ファイルの名前は``config.ini``です。
最初の行に [open_jtalk_config] が書かれているものだけが設定ファイルとして認識されます。

このファイルの場所は、実行ファイルと同じフォルダです。

Windowsでは実行ファイルと同じ場所だけでなく、dllと同じ場所にも設定ファイルを置くことができます。
実行ファイルの側に設定ファイルがあればそれを優先し、実行ファイルの側に無くdllの側にあれば、それを使います。
jtalk.dllだけでなく、jtalkcom.dllも対象になります。

### 指定できる情報

以下の情報を設定できます。
``:``の後は型を表しています。

#### 音響モデルと辞書のパスの指定

* voice_dir : 文字列
* dic_dir : 文字列
* voice : 文字列

voice_dir、dic_dir はフォルダを指定します。
絶対パス、相対パスのどちらでもかまいません。
相対パスは設定ファイルのある場所からの相対位置になります。

voiceはhtsvoice拡張子の音響モデルファイルの指定です。
絶対パス、相対パス、名前の三種類の指定ができます。
絶対パスはファイルの位置そのものの文字列です。
相対パスは設定ファイルのある場所からの相対位置になります。
名前は、voice_dir内に含まれるファイルの中から探索された最初のファイルを表します。

#### 音響モデルのパラメータの指定

open_jtalk のコマンドラインのオプションで指定できる音響モデルのパラメータです。
括弧内はオプションに使われている別名です。

* sampling_frequency(s) : 整数
* fperiod(p) : 整数
* alpha(a) : 浮動小数点数
* beta(b) : 浮動小数点数
* speed(r) : 浮動小数点数
* additional_half_tone(fm) : 浮動小数点数
* msd_threshold(u) : 浮動小数点数
* gv_weight_for_spectrum(jm) : 浮動小数点数
* gv_weight_for_log_f0(jf) : 浮動小数点数
* volume(g) : 浮動小数点数

### 値が適応されるタイミング

このファイルが実行されるのは初期化処理が行われる``openjtalk_initialize``の内部です。
また初期状態に戻す``openjtalk_refresh``が呼び出されたときも、実行されます。

内容は、書かれている順序で適応されます。
したがって、voice を設定すると、音響モデルファイルの性質上、その前の行までに指定された音響モデルのパラメータがリセットされます。
また、同じ名前に複数回、値が指定されているときは、上から順に適応され、結果的に最後の値だけが有効となります。

初期化関数 openjtalk_initializeでは、voice、dic_dir、voice_dir が引数として指定可能ですが、
これらの値が``""``か``NULL``のとき、存在しない値が指定されたとき、設定ファイルのvoice、dic_dir、voice_dirが適応されます。

したがって、この設定ファイルで音響モデルのパラメータ指定が意味を持つのは限定的になります。
openjtalk_initializeで引数voiceの値が省略されていて、代わりに設定ファイルで標準の音響モデルとそれに対するパラメータの設定を記述しなけらばならないといった場面に限られます。

### 書式の解説

このファイルで使われる書式はini形式に近いものです。
文字コードをUTF-8に限定しています。
BOM付き、BOM無し両方対応しています。

値の指定は``名前 = 値``の形です。一行に一つだけです。
名前と値の前後にあるタブとスペースは無視されます。

値には型があり、型は文字列、整数、浮動小数点数、論理値です。

文字列は 引用符``"``で囲まれたものです。  
文字列中のバックスラッシュそのものはエスケープされ、``\\``で表します。
Windowsのパス文字列中のバックスラッシュも例外ではありません。

整数は、先頭に``-``か省略可能な``+``があり、その後に数字の並びが続くものです。
小数点を含まないので、間違えて後述の浮動小数点数が記述された場合は、小数点より前が値となり、その後ろの記述は異常なものとして無視されます。
内部での値はC言語のlongです。

浮動小数点数は、先頭に``-``か省略可能な``+``があり、その後に途中に1つもしくは0個の小数点を含む数字の並びが続くものです。
これは内部ではC言語のdoubleとして扱われます。
小数点を含まない場合は、整数と区別がつきませんが、この場合も、doubleとして扱われます。

論理値は true と false のどちらかです。

``#`` もしくは ``;`` の後から改行まではコメントになります。

### 設定の例

```plain:
open_jtalk/
　├ bin/
　│　└ config.ini
　├ voice/
　└ dic/
```

上記のようにフォルダが配置されているとき、
binフォルダにある実行ファイル用のconfig.iniにおいて、
音響モデルフォルダと辞書フォルダを指定するには、
以下のように記述します。

```ini:
[open_jtalk_config]
# 音響モデルフォルダ
voice_dir = ..\\voice

# 辞書フォルダ
dic_dir = ..\\dic
```


## ライセンス

このプロジェクトにあるオリジナルのファイルのライセンスは、[MIT ライセンス](https://opensource.org/licenses/MIT)とします。
それらは、主に[jtalkフォルダ](https://github.com/rosmarinus/jtalkdll/tree/master/jtalk)、
[ffiフォルダ](https://github.com/rosmarinus/jtalkdll/tree/master/ffi)にあります。

このリポジトリのファイルには、オリジナルのファイル以外に、他のプロジェクトのファイルを含んでいます。
そのまま使っているものもあれば、一部修正を加えているものもあります。
他のプロジェクト由来のファイルは、それぞれのライセンスに従います。
