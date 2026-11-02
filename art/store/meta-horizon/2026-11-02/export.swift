// Exportación técnica en macOS: ajusta proporción/dimensiones y escribe RGB/RGBA.
// Uso: swift export.swift entrada salida ancho alto [alpha]
import Foundation
import CoreGraphics
import ImageIO
import UniformTypeIdentifiers
let args = CommandLine.arguments
 guard args.count >= 5, let w = Int(args[3]), let h = Int(args[4]),
       let source = CGImageSourceCreateWithURL(URL(fileURLWithPath: args[1]) as CFURL, nil),
       let image = CGImageSourceCreateImageAtIndex(source, 0, nil) else { fatalError("input output width height [alpha]") }
let alpha = args.count > 5 && args[5] == "alpha"
let mode = alpha ? CGImageAlphaInfo.premultipliedLast : CGImageAlphaInfo.noneSkipLast
let context = CGContext(data: nil, width: w, height: h, bitsPerComponent: 8,
    bytesPerRow: w*4, space: CGColorSpace(name: CGColorSpace.sRGB)!, bitmapInfo: mode.rawValue)!
context.interpolationQuality = .high
let sw = CGFloat(image.width), sh = CGFloat(image.height)
let scale = alpha ? min(CGFloat(w)/sw, CGFloat(h)/sh) : max(CGFloat(w)/sw, CGFloat(h)/sh)
context.draw(image, in: CGRect(x: (CGFloat(w)-sw*scale)/2, y: (CGFloat(h)-sh*scale)/2,
                             width: sw*scale, height: sh*scale))
let output = CGImageDestinationCreateWithURL(URL(fileURLWithPath: args[2]) as CFURL, UTType.png.identifier as CFString, 1, nil)!
CGImageDestinationAddImage(output, context.makeImage()!, nil)
guard CGImageDestinationFinalize(output) else { fatalError("PNG export failed") }
