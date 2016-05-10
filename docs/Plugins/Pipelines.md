# Pipelines
Pipelines in Sitecore Rocks are similar to pipelines in Sitecore. The implementation is a bit different but the 
concept is the same. A pipeline consists of a number of pipeline processors that are executed in sequence.

A pipeline is a class that inherits from the Pipeline<T> class where T is the class itself:

```
public class DeleteItemPipeline : Pipeline<DeleteItemPipeline>
```

The pipeline class defines the parameters that are used when processing. The class does not need an 
attribute.

Each pipeline processor must be marked with the [Pipeline] attribute and inherit from the 
PipelineProcessor<T> class where T is the pipeline.

```
[Pipeline(typeof(DeleteItemPipeline), 1000)]
public class SetName : PipelineProcessor<DeleteItemPipeline>
```

The [Pipeline] attribute takes 2 parameters - the type of the pipeline and a sorting value.
The PipelineProcessor class defines the abstract method Process which takes the pipeline type as a 
generic argument:

```
protected override void Process(DeleteItemPipeline pipeline)
```

To instantiate a pipeline, use the PipelineManager.GetPipeline<T> method.

```
var pipeline = PipelineManager.GetPipeline<DeleteItemPipeline>();
pipeline.ItemUri = ...
pipeline.Start();
```
