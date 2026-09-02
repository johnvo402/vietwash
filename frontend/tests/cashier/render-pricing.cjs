require("ts-node").register({
  transpileOnly: true,
  compilerOptions: {
    module: "commonjs",
    moduleResolution: "node",
    jsx: "react-jsx",
  },
});
const React = require("react");
const { renderToStaticMarkup } = require("react-dom/server");
const {
  PricingSummary,
} = require("../../src/features/cashier/components/pricing-summary");
process.stdout.write(
  renderToStaticMarkup(
    React.createElement(PricingSummary, JSON.parse(process.argv[2])),
  ),
);
