# Bundled demo fonts

All fonts here are open source (SIL Open Font License 1.1 - see the accompanying `OFL-*.txt` files,
one per family/foundry since copyright holders differ) and are used to exercise the `@font-face` demo
sample (`TestSamples/20.Fonts decorations.htm`) and the integration test suite, without depending on
whatever fonts happen to be installed on the machine running them.

| Family | Faces | Source |
| --- | --- | --- |
| Liberation Sans/Serif/Mono | Regular, Bold, Italic, Bold Italic | https://github.com/liberationfonts/liberation-fonts, release 2.1.5 (`liberation-fonts-ttf-2.1.5.tar.gz`) |
| Noto Sans/Serif | Regular, Bold, Italic, Bold Italic | https://github.com/notofonts/notofonts.github.io, `fonts/<Family>/hinted/ttf/` |
| Pacifico | Regular | https://github.com/google/fonts, `ofl/pacifico/` |
| Dancing Script | Regular (variable font, default instance) | https://github.com/google/fonts, `ofl/dancingscript/` |
| Caveat | Regular (variable font, default instance) | https://github.com/google/fonts, `ofl/caveat/` |
| Indie Flower | Regular | https://github.com/google/fonts, `ofl/indieflower/` |
| Comic Neue | Regular, Bold, Italic, Bold Italic | https://github.com/google/fonts, `ofl/comicneue/` |

Plain TTF only (no WOFF/WOFF2) - this project's `@font-face` support doesn't decompress WOFF/WOFF2 in v1.
