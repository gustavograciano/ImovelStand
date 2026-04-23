import { defineConfig } from 'vite';

/**
 * Build do widget embedavel. Gera um unico IIFE (widget.js) e um CSS,
 * ambos carregaveis por tag <script> em qualquer site.
 *
 * Uso no site da incorporadora:
 *   <script
 *     src="https://cdn.imovelstand.com.br/widget/widget.js"
 *     data-tenant="construtora-xyz"
 *     data-api="https://api.imovelstand.com.br"
 *   ></script>
 *   <div id="imovelstand-simulador"></div>
 */
export default defineConfig({
  build: {
    lib: {
      entry: 'src/widget.ts',
      formats: ['iife'],
      name: 'ImovelStandSimulador',
      fileName: () => 'widget.js'
    },
    outDir: 'dist',
    emptyOutDir: true,
    sourcemap: false,
    minify: 'esbuild',
    target: 'es2017',
    rollupOptions: {
      output: {
        inlineDynamicImports: true,
        // CSS vai inline no JS para single-file embed
        assetFileNames: 'widget.[ext]'
      }
    }
  },
  server: {
    port: 5174
  }
});
