# Lightning QR provenance

`lightning.svg` is the same QR the author uses in Pentagrama and Plec. Reused at the author's request. It contains this LNURL, not a fixed invoice or amount:

```
LNURL1DP68GURN8GHJ7AMPD3KX2AR0VEEKZAR0WD5XJTNRDAKJ7TNHV4KXCTTTDEHHWM30D3H82UNVWQHKZUR5WDMKJMFEXQESLJTDDS
```

The corresponding displayed Lightning address is `aptswim903@walletofsatoshi.com`. This is public contribution information supplied by the author.

`lightning.modules` is the canonical 45 × 45 module matrix, including the four-module white quiet zone. `tools/BuildLightningQr.ps1` recreates it from the local SVG. Normal builds embed the matrix directly; the app draws black/white integer-sized squares and never contacts a QR service or Lightning endpoint. QR access does not initiate a payment from the app.

The original app code and rendering remain MIT; the QR encodes the author's public contribution endpoint. No payment SDK or barcode library is shipped.
