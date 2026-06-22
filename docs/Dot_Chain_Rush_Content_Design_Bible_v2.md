# DOT CHAIN RUSH - CONTENT DESIGN BIBLE v2

## Complete Dot / Orb System Specification

Bu doküman Unity üretimi için ana içerik referansıdır. Amaç: - Tüm dot
türlerini data-driven yönetmek - ScriptableObject ile koddan bağımsız
balans yapmak - Yeni dot eklemeyi kolaylaştırmak

------------------------------------------------------------------------

# GENEL DOT MİMARİSİ

Her dot şu veri yapısına sahip olmalıdır:

DotConfig

-   id
-   displayName
-   category
-   unlockLevel
-   sprite
-   animation
-   color
-   fallSpeed
-   weight
-   canChain
-   scoreMultiplier
-   destroyEffect
-   specialEffect
-   comboInteraction
-   feverInteraction

Her dot runtime olarak:

Spawn -\> Move -\> Detect -\> Chain -\> Resolve Effect -\> Destroy -\>
Return Pool

akışını kullanır.

------------------------------------------------------------------------

# KATEGORİLER

1.  Temel Toplar
2.  Özel Toplar
3.  Mekanik Toplar
4.  Element Toplar
5.  Kaos Topları
6.  Güçlendiriciler
7.  Boss Topları
8.  Prestige Topları

==================================================

# FAZ 1 - ÖĞRETME (LEVEL 1-5)

## NORMAL DOT

Level: 1

Kategori: Temel

Mantık: Ana oyun taşı.

Kurallar:

-   Aynı renkler bağlanabilir.
-   Minimum zincir 3.
-   Yok olunca skor verir.

Kod:

canChain = true

effect = none

------------------------------------------------------------------------

## RAINBOW DOT

Level: 2

Spawn: %5

Mantık:

Joker renk.

Her renkle eşleşir.

Kod:

wildcard = true

Bonus:

Büyük zincirlerde daha yüksek skor.

------------------------------------------------------------------------

## SPEED DOT

Level: 3

Mantık:

Normalden hızlı düşer.

Parametre:

fallSpeed x2

Oyuncuya öğrettiği:

Hızlı karar.

------------------------------------------------------------------------

## HEAVY DOT

Level: 4

Mantık:

Alan kaplayan ağırlıklı dot.

weight = 3

Zincir yapılmazsa Danger Zone'u hızlandırır.

==================================================

# FAZ 2 - MEKANİKLER (6-10)

## BOMB DOT

Level: 6

Bağlanamaz.

Yok olunca:

ExplosionRadius = 3

Etkisi:

Yakındaki dotları yok eder.

Combo ile güçlenir.

------------------------------------------------------------------------

## FROZEN DOT

Level: 7

canChain=false

Yanındaki patlama ile çözülür.

Durum:

Frozen

------------------------------------------------------------------------

## GRAVITY DOT

Level: 8

Etkisi:

Yakındaki dotların düşüş yönünü değiştirir.

gravityModifier

------------------------------------------------------------------------

## LOCK DOT

Level: 9

Bir rengi kilitler.

Çözüm:

KEY DOT

------------------------------------------------------------------------

## KEY DOT

Level: 9

Lock Dot temizler.

------------------------------------------------------------------------

## TIME DOT

Level: 9

Oyuncuya süre bonusu verir.

timeBonus +3 saniye

------------------------------------------------------------------------

# BOSS

## CHAOS ORB

Level: 10

3 faz:

FAZ 1: Büyür

FAZ 2: Kopya üretir

FAZ 3: Çekim uygular

==================================================

# FAZ 3 ELEMENTLER (11-20)

## FIRE DOT

Alan patlaması.

destroyEffect:

fireExplosion

------------------------------------------------------------------------

## ICE DOT

Düşüşü yavaşlatır.

slowEffect

------------------------------------------------------------------------

## NATURE DOT

Çoğalma sağlar.

spawnChild

------------------------------------------------------------------------

## VOID DOT

Yakındaki dotları emer.

absorbRadius

------------------------------------------------------------------------

## GOLD DOT

Skor:

x2

scoreMultiplier = 2

------------------------------------------------------------------------

## ELECTRIC DOT

Zincir elektrik yayar.

Bir dot daha atlar.

------------------------------------------------------------------------

## WATER DOT

Yakındaki ateşi söndürür.

------------------------------------------------------------------------

## EARTH DOT

Ağır ve dayanıklı.

------------------------------------------------------------------------

## LIGHT DOT

Ekranı temizleyen nadir güç.

==================================================

# FAZ 4 FLOW BREAKERS

## MIRROR DOT

İki taraftan spawn sistemi açar.

------------------------------------------------------------------------

## BLACK HOLE

Merkez çekimi oluşturur.

------------------------------------------------------------------------

## FAKE DOT

Yanlış renk gösterir.

Gerçek renk:

hiddenColor

------------------------------------------------------------------------

## DOUBLE DIRECTION DOT

Yukarı ve aşağı hareket etkisi.

------------------------------------------------------------------------

## STICKY DOT

Yakındaki dotları yavaşlatır.

------------------------------------------------------------------------

## TELEPORT DOT

Pozisyon değiştirir.

==================================================

# FAZ 5 MUTATION

## MUTATION DOT

Pasif güç verir.

Örnek:

+30 chain score

------------------------------------------------------------------------

## CHAIN MASTER

Combo büyütür.

------------------------------------------------------------------------

## EXPLOSION CORE

Patlama alanı artırır.

------------------------------------------------------------------------

## RAINBOW BLOOD

Rainbow oranı artırır.

------------------------------------------------------------------------

## SHIELD DOT

Bir hatayı korur.

------------------------------------------------------------------------

## LIFE DOT

Can verir.

------------------------------------------------------------------------

## 2X SCORE DOT

Skor katlar.

==================================================

# FAZ 6 CHAOS

## INVISIBLE DOT

Normalde görünmez.

Yaklaşınca görünür.

------------------------------------------------------------------------

## QUANTUM DOT

İki renk taşır.

------------------------------------------------------------------------

## GLITCH DOT

Konum değiştirir.

------------------------------------------------------------------------

## TIME LORD

Boss.

Süre azaltır.

------------------------------------------------------------------------

## REVERSE GRAVITY

Yerçekimini ters çevirir.

------------------------------------------------------------------------

## EXPLOSION RAIN

Rastgele patlama yağmuru.

==================================================

# FAZ 7 ENDGAME

## VIRUS DOT

Çoğalır.

spawnChild=true

------------------------------------------------------------------------

## GRAVITY CORE

Mini karadelik.

------------------------------------------------------------------------

## ONE MISTAKE

Yanlış zincirde:

comboReset

------------------------------------------------------------------------

## CRITICAL DOT

Kritik skor verir.

------------------------------------------------------------------------

## DEATH DOT

Büyük risk.

Patlamazsa alan bozar.

------------------------------------------------------------------------

## REALITY BREAKER

Kuralları değiştirir.

------------------------------------------------------------------------

# PRESTIGE

## CORRUPTED BOMB

Daha büyük patlama.

## VOID RAINBOW

Tüm renkleri bağlar.

## ELITE HEAVY

Çok ağır.

## INFINITE TIME

Süre sistemi bozulur.

## CHAOS CORE

Rastgele özel efekt.

## OMEGA

Prestige boss.

==================================================

# NADİR TOPLAR

## COMBO DOT

Büyük combo ödülü.

## MAGNET DOT

Aynı renkleri çeker.

## JACKPOT DOT

Çok yüksek ödül.

## WIND DOT

Akışı değiştirir.

## FREEZE AREA

Alan dondurur.

## LASER DOT

Satır temizler.

## ENERGY DOT

Özel güç doldurur.

## MULTIPLY DOT

Kopyalar.

## BLACK LIGHT

Gizli renkleri gösterir.

## STAR DOT

Bonus puan.

## NUCLEAR DOT

Büyük patlama.

## DIMENSION GATE

Yeni alan açar.

## HYPER DOT

Hız moduna geçer.

## INFINITY DOT

Sonsuz zincir etkisi.

## COSMIC DOT

Rastgele kozmik efekt.

==================================================

# COMBINATION SYSTEM

Bomb + Fire

= Mega Explosion

Ice + Water

= Freeze Area

Rainbow + Electric

= Color Lightning

Black Hole + Multiply

= Singularity

Gold + 2X

= Super Gold

Time + Time

= Time Stop

==================================================

# UNITY IMPLEMENTATION KURALI

Her dot:

ScriptableObject olmalı.

Runtime:

DotBehaviour

sadece davranışı çalıştırır.

Efekt sistemi:

IDotEffect

Örnek:

ExplosionEffect

FreezeEffect

GravityEffect

Yeni dot eklemek:

1 Sprite ekle 2 DotConfig oluştur 3 Effect bağla

Kod değiştirme yok.

==================================================

# HEDEF

Oyuncu her 5-10 levelde yeni bir sürpriz görmeli.

Oyun hissi:

Kolay öğren Zor ustalaş Sürekli yeni mekanik aç
